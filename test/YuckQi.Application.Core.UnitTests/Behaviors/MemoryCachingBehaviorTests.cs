using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using YuckQi.Application.Core.Abstract.Aspects.Interfaces;
using YuckQi.Application.Core.Behaviors.Caching;

namespace YuckQi.Application.Core.UnitTests.Behaviors;

public class MemoryCachingBehaviorTests
{
    [Test]
    public async Task Handle_WhenCacheHit_ReturnsCachedResponseAndDoesNotCallNext()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var options = Options.Create(new MemoryCachingBehaviorOptions(null));
        var logger = new Mock<ILogger<MemoryCachingBehavior<CacheablePingRequest, Int32>>>();
        var behavior = new MemoryCachingBehavior<CacheablePingRequest, Int32>(memoryCache, options, logger.Object);
        var request = new CacheablePingRequest();
        var expected = 42;
        var next = false;

        memoryCache.Set(request.CacheKey, expected);

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
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var options = Options.Create(new MemoryCachingBehaviorOptions(TimeSpan.FromMinutes(5)));
        var logger = new Mock<ILogger<MemoryCachingBehavior<CacheablePingRequest, Int32>>>();
        var behavior = new MemoryCachingBehavior<CacheablePingRequest, Int32>(memoryCache, options, logger.Object);
        var request = new CacheablePingRequest();
        var expected = 7;

        var result = await behavior.Handle(request, t => Next(), CancellationToken.None);

        Assert.That(result, Is.EqualTo(expected));

        Assert.That(memoryCache.TryGetValue(request.CacheKey, out var cached));
        Assert.That(cached, Is.EqualTo(expected));

        Task<Int32> Next() => Task.FromResult(expected);
    }

    public sealed class CacheablePingRequest : IRequest<Int32>, IHasCacheKey
    {
        public String CacheKey { get; set; } = "ping";
    }
}
