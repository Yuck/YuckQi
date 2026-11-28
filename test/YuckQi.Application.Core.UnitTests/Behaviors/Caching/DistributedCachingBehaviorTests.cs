using System.Text;
using System.Text.Json;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using YuckQi.Application.Core.Aspects.Abstract.Interfaces;
using YuckQi.Application.Core.Behaviors.Caching;
using YuckQi.Application.Core.Behaviors.Validation;
using YuckQi.Domain.Validation;

namespace YuckQi.Application.Core.UnitTests.Behaviors.Caching;

public class DistributedCachingBehaviorTests
{
    [Test]
    public async Task Handle_WhenCacheHit_ReturnsCachedResponseAndDoesNotCallNext()
    {
        var cache = CreateCache();
        var behavior = CreateBehavior<Int32>(cache, new DistributedCachingBehaviorOptions());
        var request = new CacheablePingRequest();
        var calls = 0;

        await behavior.Handle(request, (t, u) => Next(), CancellationToken.None);

        var result = await behavior.Handle(request, (t, u) => Next(), CancellationToken.None);

        Assert.That(result, Is.EqualTo(42));
        Assert.That(calls, Is.EqualTo(1));

        ValueTask<Int32> Next()
        {
            calls++;

            return new ValueTask<Int32>(42);
        }
    }

    [Test]
    public async Task Handle_WhenCacheMiss_CallsNextAndCachesResponse()
    {
        var cache = new Mock<IDistributedCache>();
        var behavior = CreateBehavior<Int32>(cache.Object, new DistributedCachingBehaviorOptions(TimeSpan.FromMinutes(5)));
        var request = new CacheablePingRequest();
        var expected = 7;

        cache.Setup(t => t.GetAsync(request.CacheKey, It.IsAny<CancellationToken>())).ReturnsAsync((Byte[]?) null);

        var result = await behavior.Handle(request, (t, u) => new ValueTask<Int32>(expected), CancellationToken.None);

        Assert.That(result, Is.EqualTo(expected));

        cache.Verify(t => t.SetAsync(request.CacheKey, It.Is<Byte[]>(u => Encoding.UTF8.GetString(u).Contains(expected.ToString(), StringComparison.Ordinal)), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_WhenAnotherResponseTypeIsCachedUnderKey_DoesNotReadIt()
    {
        var cache = CreateCache();
        var summaryBehavior = CreateBehavior<CustomerSummary>(cache, new DistributedCachingBehaviorOptions());
        var detailBehavior = CreateBehavior<CustomerDetail>(cache, new DistributedCachingBehaviorOptions());
        var request = new CacheablePingRequest { CacheKey = "customer:42" };
        var detail = new CustomerDetail(42, "someone@example.com");
        var next = false;

        await summaryBehavior.Handle(request, (t, u) => new ValueTask<CustomerSummary>(new CustomerSummary(42, "Someone")), CancellationToken.None);

        var result = await detailBehavior.Handle(request, (t, u) => Next(), CancellationToken.None);

        Assert.That(result, Is.EqualTo(detail));
        Assert.That(next, Is.True);

        ValueTask<CustomerDetail> Next()
        {
            next = true;

            return new ValueTask<CustomerDetail>(detail);
        }
    }

    [Test]
    public async Task Handle_WhenEntryIsNotInCachedFormat_TreatsAsMiss()
    {
        var cache = CreateCache();
        var behavior = CreateBehavior<CustomerDetail>(cache, new DistributedCachingBehaviorOptions());
        var request = new CacheablePingRequest { CacheKey = "customer:42" };
        var detail = new CustomerDetail(42, "someone@example.com");

        await cache.SetAsync(request.CacheKey, JsonSerializer.SerializeToUtf8Bytes(new CustomerSummary(42, "Someone")));

        var result = await behavior.Handle(request, (t, u) => new ValueTask<CustomerDetail>(detail), CancellationToken.None);

        Assert.That(result, Is.EqualTo(detail));
    }

    [Test]
    public async Task Handle_WhenResponseHasValidationErrors_DoesNotCacheResponse()
    {
        var cache = new Mock<IDistributedCache>();
        var behavior = CreateBehavior<ValidationResponse<String>>(cache.Object, new DistributedCachingBehaviorOptions());
        var request = new CacheablePingRequest();
        var failed = new ValidationResponse<String> { ValidationResults = [new Result([new ResultDetail("Invalid.")])] };

        var result = await behavior.Handle(request, (t, u) => new ValueTask<ValidationResponse<String>>(failed), CancellationToken.None);

        Assert.That(result, Is.SameAs(failed));

        cache.Verify(t => t.SetAsync(It.IsAny<String>(), It.IsAny<Byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenResponseHasOnlyWarningsAndDefaultSerializerOptions_CachesResponse()
    {
        var cache = CreateCache();
        var behavior = CreateBehavior<ValidationResponse<String>>(cache, new DistributedCachingBehaviorOptions());
        var request = new CacheablePingRequest();
        var warned = new ValidationResponse<String> { Value = "value", ValidationResults = [new Result([new ResultDetail("Careful.", "careful", "Name", ResultType.Warning)])] };

        await behavior.Handle(request, (t, u) => new ValueTask<ValidationResponse<String>>(warned), CancellationToken.None);

        var result = await behavior.Handle(request, (t, u) => throw new InvalidOperationException(), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo("value"));
        Assert.That(result.ValidationResults, Is.EqualTo(warned.ValidationResults));
    }

    [Test]
    public async Task Handle_WhenResponseLosesStateInRoundTrip_DoesNotCacheResponse()
    {
        var cache = new Mock<IDistributedCache>();
        var behavior = CreateBehavior<PrivateSetterResponse>(cache.Object, new DistributedCachingBehaviorOptions());
        var request = new CacheablePingRequest();
        var response = PrivateSetterResponse.Create("original");

        var result = await behavior.Handle(request, (t, u) => new ValueTask<PrivateSetterResponse>(response), CancellationToken.None);

        Assert.That(result, Is.SameAs(response));

        cache.Verify(t => t.SetAsync(It.IsAny<String>(), It.IsAny<Byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenResponseIsNull_DoesNotCacheResponse()
    {
        var cache = new Mock<IDistributedCache>();
        var behavior = CreateBehavior<String?>(cache.Object, new DistributedCachingBehaviorOptions());
        var request = new CacheablePingRequest();

        var result = await behavior.Handle(request, (t, u) => new ValueTask<String?>(default(String)), CancellationToken.None);

        Assert.That(result, Is.Null);

        cache.Verify(t => t.SetAsync(It.IsAny<String>(), It.IsAny<Byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenGetIsCanceled_ThrowsAndDoesNotCallNext()
    {
        var cache = new Mock<IDistributedCache>();
        var behavior = CreateBehavior<Int32>(cache.Object, new DistributedCachingBehaviorOptions());
        var request = new CacheablePingRequest();
        using var source = new CancellationTokenSource();
        var next = false;

        source.Cancel();
        cache.Setup(t => t.GetAsync(request.CacheKey, It.IsAny<CancellationToken>())).ThrowsAsync(new OperationCanceledException(source.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () => await behavior.Handle(request, (t, u) => Next(), source.Token));
        Assert.That(next, Is.False);

        ValueTask<Int32> Next()
        {
            next = true;

            return new ValueTask<Int32>(0);
        }
    }

    [Test]
    public async Task Handle_WhenSetIsCanceled_Throws()
    {
        var cache = new Mock<IDistributedCache>();
        var behavior = CreateBehavior<Int32>(cache.Object, new DistributedCachingBehaviorOptions());
        var request = new CacheablePingRequest();
        using var source = new CancellationTokenSource();

        cache.Setup(t => t.GetAsync(request.CacheKey, It.IsAny<CancellationToken>())).ReturnsAsync((Byte[]?) null);
        cache.Setup(t => t.SetAsync(request.CacheKey, It.IsAny<Byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>())).ThrowsAsync(new OperationCanceledException(source.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () => await behavior.Handle(request, (t, u) => Next(), source.Token));

        ValueTask<Int32> Next()
        {
            source.Cancel();

            return new ValueTask<Int32>(7);
        }
    }

    [Test]
    public async Task Handle_WhenGetFails_CallsNext()
    {
        var cache = new Mock<IDistributedCache>();
        var behavior = CreateBehavior<Int32>(cache.Object, new DistributedCachingBehaviorOptions());
        var request = new CacheablePingRequest();

        cache.Setup(t => t.GetAsync(request.CacheKey, It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException());

        var result = await behavior.Handle(request, (t, u) => new ValueTask<Int32>(7), CancellationToken.None);

        Assert.That(result, Is.EqualTo(7));
    }

    private static DistributedCachingBehavior<CacheablePingRequest, TResponse> CreateBehavior<TResponse>(IDistributedCache cache, DistributedCachingBehaviorOptions options)
    {
        var logger = new Mock<ILogger<DistributedCachingBehavior<CacheablePingRequest, TResponse>>>();

        return new DistributedCachingBehavior<CacheablePingRequest, TResponse>(cache, Options.Create(options), logger.Object);
    }

    private static MemoryDistributedCache CreateCache()
    {
        return new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
    }

    public sealed class CacheablePingRequest : IRequest<Int32>, IHasCacheKey
    {
        public CacheKey CacheKey { get; set; } = "ping";
    }

    public sealed record CustomerDetail(Int32 Id, String Email);

    public sealed record CustomerSummary(Int32 Id, String Name);

    public sealed class PrivateSetterResponse
    {
        public String? Name { get; private set; }

        public static PrivateSetterResponse Create(String name)
        {
            return new PrivateSetterResponse { Name = name };
        }
    }
}
