using System.Diagnostics;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YuckQi.Application.Core.Aspects.Abstract.Interfaces;
using YuckQi.Application.Core.Behaviors.Caching.Internal;

namespace YuckQi.Application.Core.Behaviors.Caching;

public class DistributedCachingBehavior<TRequest, TResponse>(IDistributedCache cache, IOptions<DistributedCachingBehaviorOptions> configuration, ILogger<DistributedCachingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage, IHasCacheKey
{
    public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var type = typeof(TRequest).Name;
        var key = request.CacheKey;
        var settings = configuration.Value;

        try
        {
            var cached = await cache.GetAsync(key, cancellationToken);
            if (cached is { Length: > 0 } && CachedResponse.TryDeserialize<TResponse>(cached, settings.SerializerOptions, out var result))
            {
                logger.LogInformation("Distributed cache hit for '{type}' with key '{key}' ({elapsed:g} elapsed).", type, key, stopwatch.Elapsed);

                return result;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Distributed cache get failed for '{type}' with key '{key}'.", type, key);
        }

        logger.LogInformation("Distributed cache miss for '{type}' with key '{key}'.", type, key);

        var response = await next(request, cancellationToken);
        if (response is null)
            return response;

        if (CachedResponse.HasValidationErrors(response))
        {
            logger.LogInformation("Distributed cache set skipped for '{type}' with key '{key}' because the response has validation errors ({elapsed:g} elapsed).", type, key, stopwatch.Elapsed);

            return response;
        }

        var options = new DistributedCacheEntryOptions();
        var cacheDuration = settings.CacheDuration;
        if (cacheDuration.HasValue)
            options.SetAbsoluteExpiration(cacheDuration.Value);

        try
        {
            if (! CachedResponse.TrySerialize(response, settings.SerializerOptions, out var bytes))
            {
                logger.LogWarning("Distributed cache set skipped for '{type}' with key '{key}' because '{response}' does not round-trip through System.Text.Json ({elapsed:g} elapsed).", type, key, typeof(TResponse).Name, stopwatch.Elapsed);

                return response;
            }

            await cache.SetAsync(key, bytes, options, cancellationToken);

            logger.LogInformation("Distributed cache set for '{type}' with key '{key}' ({elapsed:g} elapsed).", type, key, stopwatch.Elapsed);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Distributed cache set failed for '{type}' with key '{key}' ({elapsed:g} elapsed).", type, key, stopwatch.Elapsed);
        }

        return response;
    }
}
