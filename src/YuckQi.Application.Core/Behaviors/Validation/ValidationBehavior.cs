using System.Diagnostics;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.Logging;
using YuckQi.Application.Core.Aspects.Abstract.Interfaces;
using YuckQi.Application.Core.Behaviors.Validation.Internal;
using YuckQi.Domain.Validation;
using YuckQi.Domain.Validation.Extensions;

namespace YuckQi.Application.Core.Behaviors.Validation;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage where TResponse : IHasValidationResults, new()
{
    public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var type = typeof(TRequest).Name;
        var configured = validators.ToList();

        logger.LogInformation("Validation of '{type}' started.", type);

        if (configured.Count == 0)
        {
            logger.LogInformation("Validation of '{type}' does not have any validators configured.", type);

            return await next(request, cancellationToken);
        }

        var results = await GetReportedResults(configured, request, cancellationToken);
        if (results.Any(t => ! t.IsValid))
        {
            logger.LogInformation("Validation of '{type}' failed ({elapsed:g} elapsed).", type, stopwatch.Elapsed);

            return new TResponse { ValidationResults = results };
        }

        logger.LogInformation("Validation of '{type}' completed ({elapsed:g} elapsed).", type, stopwatch.Elapsed);

        var response = await next(request, cancellationToken);
        if (results.Count == 0)
            return response;
        if (IsMissing(response))
            return response;

        return WithResults(response, results);
    }

    private static async Task<IReadOnlyCollection<Result>> GetReportedResults(IEnumerable<IValidator<TRequest>> validators, TRequest request, CancellationToken cancellationToken)
    {
        var results = new List<Result>();

        foreach (var validator in validators)
        {
            var result = await validator.GetResult(request, cancellationToken);
            if (result.Detail.Count > 0)
                results.Add(result);
        }

        return results;
    }

    private static Boolean IsMissing(Object? response)
    {
        return response is null;
    }

    private static TResponse WithResults(TResponse response, IReadOnlyCollection<Result> results)
    {
        var copy = ShallowCopy.Of(response);
        var existing = response.ValidationResults;
        if (existing is null)
            copy.ValidationResults = results;
        else
            copy.ValidationResults = [.. existing, .. results];

        return copy;
    }
}
