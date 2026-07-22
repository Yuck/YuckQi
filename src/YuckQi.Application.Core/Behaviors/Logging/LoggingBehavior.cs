using System.Diagnostics;
using Mediator;
using Microsoft.Extensions.Logging;

namespace YuckQi.Application.Core.Behaviors.Logging;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : IBaseRequest
{
    public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var type = typeof(TRequest).Name;

        logger.LogInformation("Handling '{type}' started.", type);

        var response = await next(request, cancellationToken);

        logger.LogInformation("Handling '{type}' completed ({elapsed:g} elapsed).", type, stopwatch.Elapsed);

        return response;
    }
}
