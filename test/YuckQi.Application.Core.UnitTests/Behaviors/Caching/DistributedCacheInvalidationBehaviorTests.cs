using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using YuckQi.Application.Core.Aspects.Abstract.Interfaces;
using YuckQi.Application.Core.Behaviors.Caching;
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph;
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Factories;

namespace YuckQi.Application.Core.UnitTests.Behaviors.Caching;

public class DistributedCacheInvalidationBehaviorTests
{
    [Test]
    public async Task Handle_AfterNext_RemovesAllKeysFromCache()
    {
        var cache = new Mock<IDistributedCache>();
        var logger = new Mock<ILogger<DistributedCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>>>();
        var behavior = new DistributedCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>(cache.Object, CacheDependencyGraph.Empty, logger.Object);
        var response = new InvalidationPingResponse(99, new HashSet<CacheKey> { "key1", "key2" });

        cache.Setup(t => t.RemoveAsync("key1", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        cache.Setup(t => t.RemoveAsync("key2", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await behavior.Handle(new InvalidationPingRequest(), (t, u) => new ValueTask<InvalidationPingResponse>(response), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo(99));

        cache.Verify(t => t.RemoveAsync("key1", It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(t => t.RemoveAsync("key2", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_WhenKeysNull_DoesNotThrowAndReturnsResponse()
    {
        var cache = new Mock<IDistributedCache>();
        var logger = new Mock<ILogger<DistributedCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>>>();
        var behavior = new DistributedCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>(cache.Object, CacheDependencyGraph.Empty, logger.Object);
        var response = new InvalidationPingResponse(3, null!);

        var result = await behavior.Handle(new InvalidationPingRequest(), (t, u) => new ValueTask<InvalidationPingResponse>(response), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo(3));

        cache.Verify(t => t.RemoveAsync(It.IsAny<String>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenKeysEmpty_ReturnsResponseWithoutRemovingAnything()
    {
        var cache = new Mock<IDistributedCache>();
        var logger = new Mock<ILogger<DistributedCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>>>();
        var behavior = new DistributedCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>(cache.Object, CacheDependencyGraph.Empty, logger.Object);
        var response = new InvalidationPingResponse(0, new HashSet<CacheKey>());

        var result = await behavior.Handle(new InvalidationPingRequest(), (t, u) => new ValueTask<InvalidationPingResponse>(response), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo(0));

        cache.Verify(t => t.RemoveAsync(It.IsAny<String>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenDependencyGraphConfigured_RemovesExpandedKeys()
    {
        var cache = new Mock<IDistributedCache>();
        cache.Setup(t => t.RemoveAsync(It.IsAny<String>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var logger = new Mock<ILogger<DistributedCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>>>();
        var graph = CacheDependencyGraph.Create(t => t.When("order", u => u.InvalidatesGlobal("order-list")
                                                                           .InvalidatesFromParameter("customer-summary", "customer")));
        var behavior = new DistributedCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>(cache.Object, graph, logger.Object);
        var seed = CacheKeyFactory.Create("order", 42, ("customer", 7));
        var response = new InvalidationPingResponse(1, new HashSet<CacheKey> { seed });

        await behavior.Handle(new InvalidationPingRequest(), (t, u) => new ValueTask<InvalidationPingResponse>(response), CancellationToken.None);

        cache.Verify(t => t.RemoveAsync(seed, It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(t => t.RemoveAsync("order-list", It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(t => t.RemoveAsync("customer-summary:7", It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(t => t.RemoveAsync(It.IsAny<String>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    public sealed class InvalidationPingRequest : IRequest<InvalidationPingResponse>;

    public sealed record InvalidationPingResponse(Int32 Value, IReadOnlySet<CacheKey>? CacheKeys) : IHasCacheInvalidationKeys
    {
        IReadOnlySet<CacheKey> IHasCacheInvalidationKeys.CacheKeys => CacheKeys ?? new HashSet<CacheKey>();
    }
}
