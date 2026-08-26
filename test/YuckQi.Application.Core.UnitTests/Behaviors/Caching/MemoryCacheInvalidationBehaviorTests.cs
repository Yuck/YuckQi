using Mediator;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using YuckQi.Application.Core.Aspects.Abstract.Interfaces;
using YuckQi.Application.Core.Behaviors.Caching;
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph;
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Factories;

namespace YuckQi.Application.Core.UnitTests.Behaviors.Caching;

public class MemoryCacheInvalidationBehaviorTests
{
    [Test]
    public async Task Handle_AfterNext_RemovesAllKeysFromCache()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<MemoryCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>>>();
        var behavior = new MemoryCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>(memoryCache, CacheDependencyGraph.Empty, logger.Object);
        var response = new InvalidationPingResponse(99, new HashSet<CacheKey> { "key1", "key2" });

        memoryCache.Set("key1", 1);
        memoryCache.Set("key2", 2);

        var result = await behavior.Handle(new InvalidationPingRequest(), (t, u) => new ValueTask<InvalidationPingResponse>(response), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo(99));
        Assert.That(memoryCache.TryGetValue("key1", out _), Is.False);
        Assert.That(memoryCache.TryGetValue("key2", out _), Is.False);
    }

    [Test]
    public async Task Handle_WhenKeysNull_DoesNotThrowAndReturnsResponse()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<MemoryCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>>>();
        var behavior = new MemoryCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>(memoryCache, CacheDependencyGraph.Empty, logger.Object);
        var response = new InvalidationPingResponse(3, null!);

        var result = await behavior.Handle(new InvalidationPingRequest(), (t, u) => new ValueTask<InvalidationPingResponse>(response), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo(3));
    }

    [Test]
    public async Task Handle_WhenKeysEmpty_ReturnsResponseWithoutRemovingAnything()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<MemoryCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>>>();
        var behavior = new MemoryCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>(memoryCache, CacheDependencyGraph.Empty, logger.Object);
        var response = new InvalidationPingResponse(0, new HashSet<CacheKey>());

        memoryCache.Set("other", 1);

        var result = await behavior.Handle(new InvalidationPingRequest(), (t, u) => new ValueTask<InvalidationPingResponse>(response), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo(0));
        Assert.That(memoryCache.TryGetValue("other", out var cached) && cached is Int32 i && i == 1);
    }

    [Test]
    public async Task Handle_WhenDependencyGraphConfigured_RemovesExpandedKeys()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<MemoryCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>>>();
        var graph = CacheDependencyGraph.Create(t => t.When("order", u => u.InvalidatesGlobal("order-list")
                                                                           .InvalidatesFromParameter("customer-summary", "customer")));
        var behavior = new MemoryCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>(memoryCache, graph, logger.Object);
        var seed = CacheKeyFactory.Create("order", 42, ("customer", 7));
        var response = new InvalidationPingResponse(1, new HashSet<CacheKey> { seed });

        memoryCache.Set((String) seed, 1);
        memoryCache.Set("order-list", 2);
        memoryCache.Set("customer-summary:7", 3);
        memoryCache.Set("untouched", 4);

        await behavior.Handle(new InvalidationPingRequest(), (t, u) => new ValueTask<InvalidationPingResponse>(response), CancellationToken.None);

        Assert.That(memoryCache.TryGetValue((String) seed, out _), Is.False);
        Assert.That(memoryCache.TryGetValue("order-list", out _), Is.False);
        Assert.That(memoryCache.TryGetValue("customer-summary:7", out _), Is.False);
        Assert.That(memoryCache.TryGetValue("untouched", out var cached) && cached is Int32 i && i == 4);
    }

    public sealed class InvalidationPingRequest : IRequest<InvalidationPingResponse>;

    public sealed record InvalidationPingResponse(Int32 Value, IReadOnlySet<CacheKey>? CacheKeys) : IHasCacheInvalidationKeys
    {
        IReadOnlySet<CacheKey> IHasCacheInvalidationKeys.CacheKeys => CacheKeys ?? new HashSet<CacheKey>();
    }
}
