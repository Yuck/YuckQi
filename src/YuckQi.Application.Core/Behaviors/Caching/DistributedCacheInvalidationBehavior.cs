using System.Diagnostics;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using YuckQi.Application.Core.Aspects.Abstract.Interfaces;

namespace YuckQi.Application.Core.Behaviors.Caching;

public class DistributedCacheInvalidationBehavior<TRequest, TResponse>(IDistributedCache cache, ILogger<DistributedCacheInvalidationBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage where TResponse : IHasCacheInvalidationKeys
{
    public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next(request, cancellationToken);

        var keys = response.CacheKeys;
        if (keys is null)
            return response;

        var type = typeof(TResponse).Name;
        foreach (var key in keys)
        {
            if (String.IsNullOrWhiteSpace(key))
                continue;

            try
            {
                await cache.RemoveAsync(key, cancellationToken);

                logger.LogInformation("Distributed cache invalidated for '{type}' with key '{key}' ({elapsed:g} elapsed).", type, key, stopwatch.Elapsed);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Distributed cache invalidation failed for '{type}' with key '{key}' ({elapsed:g} elapsed).", type, key, stopwatch.Elapsed);
            }
        }

        return response;
    }
}
