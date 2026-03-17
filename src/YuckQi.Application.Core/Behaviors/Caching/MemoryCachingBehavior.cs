using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YuckQi.Application.Core.Aspects.Abstract.Interfaces;

namespace YuckQi.Application.Core.Behaviors.Caching;

public record MemoryCachingBehaviorOptions(TimeSpan? CacheDuration);

public class MemoryCachingBehavior<TRequest, TResponse>(IMemoryCache cache, IOptions<MemoryCachingBehaviorOptions> configuration, ILogger<MemoryCachingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IHasCacheKey
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var type = typeof(TRequest).Name;
        var key = request.CacheKey;

        try
        {
            if (cache.TryGetValue(key, out var cached) && cached is TResponse result)
            {
                logger.LogInformation("Memory cache hit for '{type}' with key '{key}' ({elapsed:g} elapsed).", type, key, stopwatch.Elapsed);

                return result;
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Memory cache get failed for '{type}' with key '{key}'.", type, key);
        }

        logger.LogInformation("Memory cache miss for '{type}' with key '{key}'.", type, key);

        var response = await next(cancellationToken);

        var options = new MemoryCacheEntryOptions();
        var cacheDuration = configuration.Value.CacheDuration;
        if (cacheDuration.HasValue)
            options.SetAbsoluteExpiration(cacheDuration.Value);

        try
        {
            cache.Set(key, response, options);

            logger.LogInformation("Memory cache set for '{type}' with key '{key}' ({elapsed:g} elapsed).", type, key, stopwatch.Elapsed);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Memory cache set failed for '{type}' with key '{key}' ({elapsed:g} elapsed).", type, key, stopwatch.Elapsed);
        }

        return response;
    }
}
