using System.Diagnostics;
using Mediator;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YuckQi.Application.Core.Aspects.Abstract.Interfaces;
using YuckQi.Application.Core.Behaviors.Caching.Internal;

namespace YuckQi.Application.Core.Behaviors.Caching;

public class MemoryCachingBehavior<TRequest, TResponse>(IMemoryCache cache, IOptions<MemoryCachingBehaviorOptions> configuration, ILogger<MemoryCachingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage, IHasCacheKey
{
    public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var type = typeof(TRequest).Name;
        var key = (String) request.CacheKey;
        var settings = configuration.Value;

        try
        {
            if (cache.TryGetValue(key, out var cached) && cached is Byte[] bytes && CachedResponse.TryDeserialize<TResponse>(bytes, settings.SerializerOptions, out var result))
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

        var response = await next(request, cancellationToken);
        if (response is null)
            return response;

        if (CachedResponse.HasValidationErrors(response))
        {
            logger.LogInformation("Memory cache set skipped for '{type}' with key '{key}' because the response has validation errors ({elapsed:g} elapsed).", type, key, stopwatch.Elapsed);

            return response;
        }

        var options = new MemoryCacheEntryOptions { Size = settings.Size };
        var cacheDuration = settings.CacheDuration;
        if (cacheDuration.HasValue)
            options.SetAbsoluteExpiration(cacheDuration.Value);

        try
        {
            if (! CachedResponse.TrySerialize(response, settings.SerializerOptions, out var bytes))
            {
                logger.LogWarning("Memory cache set skipped for '{type}' with key '{key}' because '{response}' does not round-trip through System.Text.Json ({elapsed:g} elapsed).", type, key, typeof(TResponse).Name, stopwatch.Elapsed);

                return response;
            }

            cache.Set(key, bytes, options);

            logger.LogInformation("Memory cache set for '{type}' with key '{key}' ({elapsed:g} elapsed).", type, key, stopwatch.Elapsed);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Memory cache set failed for '{type}' with key '{key}' ({elapsed:g} elapsed).", type, key, stopwatch.Elapsed);
        }

        return response;
    }
}
