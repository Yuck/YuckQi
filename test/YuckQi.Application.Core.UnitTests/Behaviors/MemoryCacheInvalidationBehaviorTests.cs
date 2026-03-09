using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using YuckQi.Application.Core.Abstract.Aspects.Interfaces;
using YuckQi.Application.Core.Behaviors;

namespace YuckQi.Application.Core.UnitTests.Behaviors;

public class MemoryCacheInvalidationBehaviorTests
{
    [Test]
    public async Task Handle_AfterNext_RemovesAllKeysFromCache()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<MemoryCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>>>();
        var behavior = new MemoryCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>(memoryCache, logger.Object);
        var response = new InvalidationPingResponse(99, new HashSet<String> { "key1", "key2" });

        memoryCache.Set("key1", 1);
        memoryCache.Set("key2", 2);

        var result = await behavior.Handle(new InvalidationPingRequest(), t => Task.FromResult(response), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo(99));
        Assert.That(memoryCache.TryGetValue("key1", out _), Is.False);
        Assert.That(memoryCache.TryGetValue("key2", out _), Is.False);
    }

    [Test]
    public async Task Handle_WhenKeysNull_DoesNotThrowAndReturnsResponse()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<MemoryCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>>>();
        var behavior = new MemoryCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>(memoryCache, logger.Object);
        var response = new InvalidationPingResponse(3, null!);

        var result = await behavior.Handle(new InvalidationPingRequest(), t => Task.FromResult(response), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo(3));
    }

    [Test]
    public async Task Handle_WhenKeysEmpty_ReturnsResponseWithoutRemovingAnything()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<MemoryCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>>>();
        var behavior = new MemoryCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>(memoryCache, logger.Object);
        var response = new InvalidationPingResponse(0, new HashSet<String>());

        memoryCache.Set("other", 1);

        var result = await behavior.Handle(new InvalidationPingRequest(), t => Task.FromResult(response), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo(0));
        Assert.That(memoryCache.TryGetValue("other", out var cached) && cached is Int32 i && i == 1);
    }

    [Test]
    public async Task Handle_WhenKeyNullOrWhiteSpace_SkipsItAndRemovesOthers()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<MemoryCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>>>();
        var behavior = new MemoryCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>(memoryCache, logger.Object);
        var response = new InvalidationPingResponse(0, new HashSet<String> { " ", "valid", "" });

        memoryCache.Set("valid", 1);

        await behavior.Handle(new InvalidationPingRequest(), t => Task.FromResult(response), CancellationToken.None);

        Assert.That(memoryCache.TryGetValue("valid", out _), Is.False);
    }

    public sealed class InvalidationPingRequest : IRequest<InvalidationPingResponse>;

    public sealed record InvalidationPingResponse(Int32 Value, IReadOnlySet<String>? CacheKeys) : IHasCacheInvalidationKeys
    {
        IReadOnlySet<String> IHasCacheInvalidationKeys.CacheKeys => CacheKeys ?? new HashSet<String>();
    }
}
