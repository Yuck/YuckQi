using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YuckQi.Application.Core.Abstract.Aspects.Interfaces;

namespace YuckQi.Application.Core.Behaviors.Caching;

public record DistributedCachingBehaviorOptions(TimeSpan? CacheDuration);

public class DistributedCachingBehavior<TRequest, TResponse>(IDistributedCache cache, IOptions<DistributedCachingBehaviorOptions> configuration, ILogger<DistributedCachingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IHasCacheKey
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var type = typeof(TRequest).Name;
        var key = request.CacheKey;

        try
        {
            var cached = await cache.GetAsync(key, cancellationToken);
            if (cached is { Length: > 0 })
            {
                var result = JsonSerializer.Deserialize<TResponse>(cached);
                if (result is not null)
                {
                    logger.LogInformation("Distributed cache hit for '{type}' with key '{key}'.", type, key);

                    return result;
                }
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Distributed cache get or deserialization failed for '{type}' with key '{key}'. Treating as cache miss.", type, key);
        }

        logger.LogInformation("Distributed cache miss for '{type}' with key '{key}'.", type, key);

        var response = await next();

        var options = new DistributedCacheEntryOptions();
        var cacheDuration = configuration.Value.CacheDuration;
        if (cacheDuration.HasValue)
            options.SetAbsoluteExpiration(cacheDuration.Value);

        try
        {
            await cache.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(response), options, cancellationToken);

            logger.LogInformation("Distributed cache set for '{type}' with key '{key}'.", type, key);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Distributed cache set failed for '{type}' with key '{key}'.", type, key);
        }

        return response;
    }
}
