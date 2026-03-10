using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using YuckQi.Application.Core.Abstract.Aspects.Interfaces;

namespace YuckQi.Application.Core.Behaviors.Caching;

public class DistributedCacheInvalidationBehavior<TRequest, TResponse>(IDistributedCache cache, ILogger<DistributedCacheInvalidationBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse> where TResponse : IHasCacheInvalidationKeys
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

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

                logger.LogInformation("Distributed cache invalidated for '{type}' with key '{key}'.", type, key);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Distributed cache invalidation failed for '{type}' with key '{key}'.", type, key);
            }
        }

        return response;
    }
}
