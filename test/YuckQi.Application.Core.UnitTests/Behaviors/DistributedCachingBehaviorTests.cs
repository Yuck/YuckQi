using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using YuckQi.Application.Core.Abstract.Aspects.Interfaces;
using YuckQi.Application.Core.Behaviors;

namespace YuckQi.Application.Core.UnitTests.Behaviors;

public class DistributedCachingBehaviorTests
{
    [Test]
    public async Task Handle_WhenCacheHit_ReturnsCachedResponseAndDoesNotCallNext()
    {
        var cache = new Mock<IDistributedCache>();
        var options = Options.Create(new DistributedCachingBehaviorOptions { CacheDuration = null });
        var logger = new Mock<ILogger<DistributedCachingBehavior<CacheablePingRequest, Int32>>>();
        var behavior = new DistributedCachingBehavior<CacheablePingRequest, Int32>(cache.Object, options, logger.Object);
        var request = new CacheablePingRequest();
        var expected = 42;
        var cachedBytes = JsonSerializer.SerializeToUtf8Bytes(expected);
        var next = false;

        cache.Setup(t => t.GetAsync(request.CacheKey, It.IsAny<CancellationToken>())).ReturnsAsync(cachedBytes);

        var result = await behavior.Handle(request, t => Next(), CancellationToken.None);

        Assert.That(result, Is.EqualTo(expected));
        Assert.That(next, Is.False);

        Task<Int32> Next()
        {
            next = true;

            return Task.FromResult(0);
        }
    }

    [Test]
    public async Task Handle_WhenCacheMiss_CallsNextAndCachesResponse()
    {
        var cache = new Mock<IDistributedCache>();
        var options = Options.Create(new DistributedCachingBehaviorOptions { CacheDuration = TimeSpan.FromMinutes(5) });
        var logger = new Mock<ILogger<DistributedCachingBehavior<CacheablePingRequest, Int32>>>();
        var behavior = new DistributedCachingBehavior<CacheablePingRequest, Int32>(cache.Object, options, logger.Object);
        var request = new CacheablePingRequest();
        var expected = 7;

        cache.Setup(t => t.GetAsync(request.CacheKey, It.IsAny<CancellationToken>())).ReturnsAsync((Byte[]?) null);

        var result = await behavior.Handle(request, t => Next(), CancellationToken.None);

        Assert.That(result, Is.EqualTo(expected));

        cache.Verify(t => t.SetAsync(request.CacheKey, It.Is<Byte[]>(u => Encoding.UTF8.GetString(u).Contains(expected.ToString(), StringComparison.Ordinal)), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);

        Task<Int32> Next() => Task.FromResult(expected);
    }

    public sealed class CacheablePingRequest : IRequest<Int32>, IHasCacheKey
    {
        public String CacheKey { get; set; } = "ping";
    }
}
