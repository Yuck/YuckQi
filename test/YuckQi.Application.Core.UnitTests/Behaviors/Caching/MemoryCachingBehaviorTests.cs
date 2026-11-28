using System.Text.Json;
using Mediator;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using YuckQi.Application.Core.Aspects.Abstract.Interfaces;
using YuckQi.Application.Core.Behaviors.Caching;
using YuckQi.Application.Core.Behaviors.Validation;
using YuckQi.Domain.Validation;
using YuckQi.Domain.Validation.Extensions;

namespace YuckQi.Application.Core.UnitTests.Behaviors.Caching;

public class MemoryCachingBehaviorTests
{
    [Test]
    public async Task Handle_WhenCacheHit_ReturnsCachedResponseAndDoesNotCallNext()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var behavior = CreateBehavior<Int32>(memoryCache, new MemoryCachingBehaviorOptions());
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
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var behavior = CreateBehavior<Int32>(memoryCache, new MemoryCachingBehaviorOptions(TimeSpan.FromMinutes(5)));
        var request = new CacheablePingRequest();
        var expected = 7;

        var result = await behavior.Handle(request, (t, u) => new ValueTask<Int32>(expected), CancellationToken.None);

        Assert.That(result, Is.EqualTo(expected));
        Assert.That(memoryCache.TryGetValue((String) request.CacheKey, out _), Is.True);
    }

    [Test]
    public async Task Handle_WhenAnotherResponseTypeIsCachedUnderKey_DoesNotReadIt()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var summaryBehavior = CreateBehavior<CustomerSummary>(memoryCache, new MemoryCachingBehaviorOptions());
        var detailBehavior = CreateBehavior<CustomerDetail>(memoryCache, new MemoryCachingBehaviorOptions());
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
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var behavior = CreateBehavior<Int32>(memoryCache, new MemoryCachingBehaviorOptions());
        var request = new CacheablePingRequest();

        memoryCache.Set((String) request.CacheKey, 42);

        var result = await behavior.Handle(request, (t, u) => new ValueTask<Int32>(7), CancellationToken.None);

        Assert.That(result, Is.EqualTo(7));
    }

    [Test]
    public async Task Handle_WhenCacheHitIsMutated_OtherCallersDoNotSeeTheChange()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var behavior = CreateBehavior<MutableResponse>(memoryCache, new MemoryCachingBehaviorOptions());
        var request = new CacheablePingRequest();

        var original = await behavior.Handle(request, (t, u) => new ValueTask<MutableResponse>(new MutableResponse { Name = "original" }), CancellationToken.None);

        original.Name = "changed by handler caller";

        var first = await behavior.Handle(request, (t, u) => throw new InvalidOperationException(), CancellationToken.None);

        first.Name = "changed by first hit";

        var second = await behavior.Handle(request, (t, u) => throw new InvalidOperationException(), CancellationToken.None);

        Assert.That(second.Name, Is.EqualTo("original"));
        Assert.That(second, Is.Not.SameAs(first));
    }

    [Test]
    public async Task Handle_WhenResponseHasValidationErrors_DoesNotCacheResponse()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var behavior = CreateBehavior<ValidationResponse<String>>(memoryCache, new MemoryCachingBehaviorOptions());
        var request = new CacheablePingRequest();
        var failed = new ValidationResponse<String> { ValidationResults = [new Result([new ResultDetail("Invalid.")])] };

        var result = await behavior.Handle(request, (t, u) => new ValueTask<ValidationResponse<String>>(failed), CancellationToken.None);

        Assert.That(result, Is.SameAs(failed));
        Assert.That(memoryCache.TryGetValue((String) request.CacheKey, out _), Is.False);
    }

    [Test]
    public async Task Handle_WhenResponseHasOnlyWarnings_CachesResponseWithSerializerOptions()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var serializerOptions = new JsonSerializerOptions().AddResultJsonConverters();
        var behavior = CreateBehavior<ValidationResponse<String>>(memoryCache, new MemoryCachingBehaviorOptions { SerializerOptions = serializerOptions });
        var request = new CacheablePingRequest();
        var warned = new ValidationResponse<String> { Value = "value", ValidationResults = [new Result([new ResultDetail("Careful.", type: ResultType.Warning)])] };

        await behavior.Handle(request, (t, u) => new ValueTask<ValidationResponse<String>>(warned), CancellationToken.None);

        var result = await behavior.Handle(request, (t, u) => throw new InvalidOperationException(), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo("value"));
        Assert.That(result.ValidationResults, Is.EqualTo(warned.ValidationResults));
    }

    [Test]
    public async Task Handle_WhenResponseHasOnlyWarningsAndDefaultSerializerOptions_CachesResponse()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var behavior = CreateBehavior<ValidationResponse<String>>(memoryCache, new MemoryCachingBehaviorOptions());
        var request = new CacheablePingRequest();
        var warned = new ValidationResponse<String> { Value = "value", ValidationResults = [new Result([new ResultDetail("Careful.", "careful", "Name", ResultType.Warning)])] };

        await behavior.Handle(request, (t, u) => new ValueTask<ValidationResponse<String>>(warned), CancellationToken.None);

        var result = await behavior.Handle(request, (t, u) => throw new InvalidOperationException(), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo("value"));
        Assert.That(result.ValidationResults, Is.EqualTo(warned.ValidationResults));
    }

    [Test]
    public async Task Handle_WhenSerializerOptionsCannotReadResponse_DoesNotCacheResponse()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var behavior = CreateBehavior<ValidationResponse<String>>(memoryCache, new MemoryCachingBehaviorOptions { SerializerOptions = new JsonSerializerOptions() });
        var request = new CacheablePingRequest();
        var warned = new ValidationResponse<String> { Value = "value", ValidationResults = [new Result([new ResultDetail("Careful.", type: ResultType.Warning)])] };

        var result = await behavior.Handle(request, (t, u) => new ValueTask<ValidationResponse<String>>(warned), CancellationToken.None);

        Assert.That(result, Is.SameAs(warned));
        Assert.That(memoryCache.TryGetValue((String) request.CacheKey, out _), Is.False);
    }

    [Test]
    public async Task Handle_WhenResponseLosesStateInRoundTrip_DoesNotCacheResponse()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var behavior = CreateBehavior<PrivateSetterResponse>(memoryCache, new MemoryCachingBehaviorOptions());
        var request = new CacheablePingRequest();
        var response = PrivateSetterResponse.Create("original");
        var calls = 0;

        await behavior.Handle(request, (t, u) => Next(), CancellationToken.None);

        var result = await behavior.Handle(request, (t, u) => Next(), CancellationToken.None);

        Assert.That(result.Name, Is.EqualTo("original"));
        Assert.That(calls, Is.EqualTo(2));
        Assert.That(memoryCache.TryGetValue((String) request.CacheKey, out _), Is.False);

        ValueTask<PrivateSetterResponse> Next()
        {
            calls++;

            return new ValueTask<PrivateSetterResponse>(response);
        }
    }

    [Test]
    public async Task Handle_WhenResponseIsDerivedFromResponseType_DoesNotCacheResponse()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var behavior = CreateBehavior<BaseResponse>(memoryCache, new MemoryCachingBehaviorOptions());
        var request = new CacheablePingRequest();
        var derived = new DerivedResponse { Name = "name", Extra = "extra" };

        var result = await behavior.Handle(request, (t, u) => new ValueTask<BaseResponse>(derived), CancellationToken.None);

        Assert.That(result, Is.SameAs(derived));
        Assert.That(memoryCache.TryGetValue((String) request.CacheKey, out _), Is.False);
    }

    [Test]
    public async Task Handle_WhenResponseTypeIsCollectionInterface_CachesResponse()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var behavior = CreateBehavior<IReadOnlyList<CustomerSummary>>(memoryCache, new MemoryCachingBehaviorOptions());
        var request = new CacheablePingRequest();
        var expected = new List<CustomerSummary> { new(1, "One"), new(2, "Two") };

        await behavior.Handle(request, (t, u) => new ValueTask<IReadOnlyList<CustomerSummary>>(expected), CancellationToken.None);

        var result = await behavior.Handle(request, (t, u) => throw new InvalidOperationException(), CancellationToken.None);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public async Task Handle_WhenResponseIsNull_DoesNotCacheResponse()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var behavior = CreateBehavior<String?>(memoryCache, new MemoryCachingBehaviorOptions());
        var request = new CacheablePingRequest();

        var result = await behavior.Handle(request, (t, u) => new ValueTask<String?>(default(String)), CancellationToken.None);

        Assert.That(result, Is.Null);
        Assert.That(memoryCache.TryGetValue((String) request.CacheKey, out _), Is.False);
    }

    [Test]
    public async Task Handle_WhenCacheHasSizeLimit_CachesResponse()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 10, TrackStatistics = true });
        var behavior = CreateBehavior<Int32>(memoryCache, new MemoryCachingBehaviorOptions { Size = 2 });
        var request = new CacheablePingRequest();
        var calls = 0;

        await behavior.Handle(request, (t, u) => Next(), CancellationToken.None);

        var result = await behavior.Handle(request, (t, u) => Next(), CancellationToken.None);

        Assert.That(result, Is.EqualTo(42));
        Assert.That(calls, Is.EqualTo(1));
        Assert.That(memoryCache.GetCurrentStatistics()?.CurrentEstimatedSize, Is.EqualTo(2));

        ValueTask<Int32> Next()
        {
            calls++;

            return new ValueTask<Int32>(42);
        }
    }

    private static MemoryCachingBehavior<CacheablePingRequest, TResponse> CreateBehavior<TResponse>(IMemoryCache cache, MemoryCachingBehaviorOptions options)
    {
        var logger = new Mock<ILogger<MemoryCachingBehavior<CacheablePingRequest, TResponse>>>();

        return new MemoryCachingBehavior<CacheablePingRequest, TResponse>(cache, Options.Create(options), logger.Object);
    }

    public class BaseResponse
    {
        public String? Name { get; set; }
    }

    public sealed class CacheablePingRequest : IRequest<Int32>, IHasCacheKey
    {
        public CacheKey CacheKey { get; set; } = "ping";
    }

    public sealed record CustomerDetail(Int32 Id, String Email);

    public sealed record CustomerSummary(Int32 Id, String Name);

    public sealed class DerivedResponse : BaseResponse
    {
        public String? Extra { get; set; }
    }

    public sealed class MutableResponse
    {
        public String? Name { get; set; }
    }

    public sealed class PrivateSetterResponse
    {
        public String? Name { get; private set; }

        public static PrivateSetterResponse Create(String name)
        {
            return new PrivateSetterResponse { Name = name };
        }
    }
}
