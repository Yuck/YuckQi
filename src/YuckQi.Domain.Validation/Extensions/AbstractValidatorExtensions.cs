using FluentValidation;
using FluentValidation.Results;

namespace YuckQi.Domain.Validation.Extensions;

public static class AbstractValidatorExtensions
{
    public static Result<T> GetResult<T>(this IValidator<T> validator, T item)
    {
        ArgumentNullException.ThrowIfNull(validator);

        var validationResult = validator.Validate(item);
        var result = BuildResult(validationResult, item);

        return result;
    }

    public static Result<T> GetResult<T>(this AbstractValidator<T> validator, T item)
    {
        return GetResult(validator as IValidator<T>, item);
    }

    public static async Task<Result<T>> GetResult<T>(this IValidator<T> validator, T item, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(validator);

        var validationResult = await validator.ValidateAsync(item, cancellationToken);
        var result = BuildResult(validationResult, item);

        return result;
    }

    public static Task<Result<T>> GetResult<T>(this AbstractValidator<T> validator, T item, CancellationToken cancellationToken)
    {
        return GetResult(validator as IValidator<T>, item, cancellationToken);
    }

    private static Result<T> BuildResult<T>(ValidationResult validationResult, T item)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        var detail = GetResultDetail(validationResult);
        var result = new Result<T>(item, detail);
        if (result.IsValid)
            return result;

        return new Result<T>(default, detail);
    }

    private static ResultCode GetResultCode(ValidationFailure failure)
    {
        if (String.IsNullOrWhiteSpace(failure.ErrorCode))
            return ResultCode.InvalidRequestDetail;

        return new ResultCode(failure.ErrorCode);
    }

    private static IReadOnlyCollection<ResultDetail> GetResultDetail(ValidationResult result)
    {
        return [..result.Errors.Select(t => new ResultDetail(t.ErrorMessage, GetResultCode(t), t.PropertyName, GetResultType(t)))];
    }

    private static ResultType GetResultType(ValidationFailure failure)
    {
        return failure.Severity switch
        {
            Severity.Warning or Severity.Info => ResultType.Warning,
            _ => ResultType.Error
        };
    }
}
