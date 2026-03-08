using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using YuckQi.Application.Core.Abstract.Interfaces;
using YuckQi.Domain.Validation;
using YuckQi.Domain.Validation.Extensions;

namespace YuckQi.Application.Core.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<AbstractValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse> where TResponse : IValidated, new()
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var type = typeof(TRequest).Name;

        logger.LogInformation("Validation of '{type}' started.", type);

        if (validators.Any())
        {
            var results = await Task.WhenAll(validators.Select(validator => validator.GetResult(request, cancellationToken)));
            var invalid = new Result(results.Where(t => ! t.IsValid).SelectMany(t => t.Detail).ToList());
            if (invalid.Detail.Any(t => t.Type == ResultType.Error))
            {
                logger.LogInformation("Validation of '{type}' failed ({elapsed:g} elapsed).", type, stopwatch.Elapsed);

                return new TResponse { ValidationResults = results };
            }
        }
        else
        {
            logger.LogInformation("Validation of '{type}' does not have any validators configured.", type);
        }

        logger.LogInformation("Validation of '{type}' completed ({elapsed:g} elapsed).", type, stopwatch.Elapsed);

        return await next();
    }
}
