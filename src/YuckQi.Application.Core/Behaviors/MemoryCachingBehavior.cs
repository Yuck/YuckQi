using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YuckQi.Application.Core.Abstract.Aspects.Interfaces;

namespace YuckQi.Application.Core.Behaviors;

public class MemoryCachingBehavior<TRequest, TResponse>(IMemoryCache cache, IOptions<MemoryCachingBehaviorOptions> configuration, ILogger<MemoryCachingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IHasCacheKey
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var type = typeof(TRequest).Name;
        var key = request.CacheKey;

        try
        {
            if (cache.TryGetValue(key, out var cached) && cached is TResponse result)
            {
                logger.LogInformation("Memory cache hit for '{type}' with key '{key}'.", type, key);

                return result;
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Memory cache get failed for '{type}' with key '{key}'. Treating as cache miss.", type, key);
        }

        logger.LogInformation("Memory cache miss for '{type}' with key '{key}'.", type, key);

        var response = await next();

        var options = new MemoryCacheEntryOptions();
        var cacheDuration = configuration.Value.CacheDuration;
        if (cacheDuration.HasValue)
            options.SetAbsoluteExpiration(cacheDuration.Value);

        try
        {
            cache.Set(key, response, options);

            logger.LogInformation("Memory cache set for '{type}' with key '{key}'.", type, key);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Memory cache set failed for '{type}' with key '{key}'.", type, key);
        }

        return response;
    }
}
