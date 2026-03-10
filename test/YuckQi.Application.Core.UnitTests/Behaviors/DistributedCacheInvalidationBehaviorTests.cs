using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using YuckQi.Application.Core.Abstract.Aspects.Interfaces;
using YuckQi.Application.Core.Behaviors.Caching;

namespace YuckQi.Application.Core.UnitTests.Behaviors;

public class DistributedCacheInvalidationBehaviorTests
{
    [Test]
    public async Task Handle_AfterNext_RemovesAllKeysFromCache()
    {
        var cache = new Mock<IDistributedCache>();
        var logger = new Mock<ILogger<DistributedCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>>>();
        var behavior = new DistributedCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>(cache.Object, logger.Object);
        var response = new InvalidationPingResponse(99, new HashSet<String> { "key1", "key2" });

        cache.Setup(t => t.RemoveAsync("key1", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        cache.Setup(t => t.RemoveAsync("key2", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await behavior.Handle(new InvalidationPingRequest(), t => Task.FromResult(response), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo(99));

        cache.Verify(t => t.RemoveAsync("key1", It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(t => t.RemoveAsync("key2", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_WhenKeysNull_DoesNotThrowAndReturnsResponse()
    {
        var cache = new Mock<IDistributedCache>();
        var logger = new Mock<ILogger<DistributedCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>>>();
        var behavior = new DistributedCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>(cache.Object, logger.Object);
        var response = new InvalidationPingResponse(3, null!);

        var result = await behavior.Handle(new InvalidationPingRequest(), t => Task.FromResult(response), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo(3));

        cache.Verify(t => t.RemoveAsync(It.IsAny<String>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenKeysEmpty_ReturnsResponseWithoutRemovingAnything()
    {
        var cache = new Mock<IDistributedCache>();
        var logger = new Mock<ILogger<DistributedCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>>>();
        var behavior = new DistributedCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>(cache.Object, logger.Object);
        var response = new InvalidationPingResponse(0, new HashSet<String>());

        var result = await behavior.Handle(new InvalidationPingRequest(), t => Task.FromResult(response), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo(0));

        cache.Verify(t => t.RemoveAsync(It.IsAny<String>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenKeyNullOrWhiteSpace_SkipsItAndRemovesOthers()
    {
        var cache = new Mock<IDistributedCache>();
        cache.Setup(t => t.RemoveAsync("valid", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var logger = new Mock<ILogger<DistributedCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>>>();
        var behavior = new DistributedCacheInvalidationBehavior<InvalidationPingRequest, InvalidationPingResponse>(cache.Object, logger.Object);
        var response = new InvalidationPingResponse(0, new HashSet<String> { " ", "valid", "" });

        await behavior.Handle(new InvalidationPingRequest(), t => Task.FromResult(response), CancellationToken.None);

        cache.Verify(t => t.RemoveAsync("valid", It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(t => t.RemoveAsync(It.IsAny<String>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public sealed class InvalidationPingRequest : IRequest<InvalidationPingResponse>;

    public sealed record InvalidationPingResponse(Int32 Value, IReadOnlySet<String>? CacheKeys) : IHasCacheInvalidationKeys
    {
        IReadOnlySet<String> IHasCacheInvalidationKeys.CacheKeys => CacheKeys ?? new HashSet<String>();
    }
}
