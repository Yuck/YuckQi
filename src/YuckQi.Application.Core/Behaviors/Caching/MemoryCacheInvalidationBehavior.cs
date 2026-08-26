using System.Diagnostics;
using Mediator;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using YuckQi.Application.Core.Aspects.Abstract.Interfaces;
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Abstract.Interfaces;

namespace YuckQi.Application.Core.Behaviors.Caching;

public class MemoryCacheInvalidationBehavior<TRequest, TResponse>(IMemoryCache cache, ICacheDependencyGraph dependencyGraph, ILogger<MemoryCacheInvalidationBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage where TResponse : IHasCacheInvalidationKeys
{
    public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next(request, cancellationToken);

        var keys = response.CacheKeys;
        if (keys is null)
            return response;

        var expanded = dependencyGraph.GetExpandedCacheKeys(keys);
        var type = typeof(TResponse).Name;
        foreach (var key in expanded)
        {
            var cacheKey = (String) key;
            if (String.IsNullOrWhiteSpace(cacheKey))
                continue;

            try
            {
                cache.Remove(cacheKey);

                logger.LogInformation("Memory cache invalidated for '{type}' with key '{key}' ({elapsed:g} elapsed).", type, cacheKey, stopwatch.Elapsed);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Memory cache invalidation failed for '{type}' with key '{key}' ({elapsed:g} elapsed).", type, cacheKey, stopwatch.Elapsed);
            }
        }

        return response;
    }
}
