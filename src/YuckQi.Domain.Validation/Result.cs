using YuckQi.Domain.Validation.Abstract.Interfaces;

namespace YuckQi.Domain.Validation;

public record Result : IResult
{
    public IReadOnlyCollection<ResultDetail> Detail { get; }

    public Boolean IsValid => Detail.All(t => t.Type == ResultType.Warning);

    public Result(IReadOnlyCollection<ResultDetail>? detail)
    {
        Detail = CopyDetail(detail);
    }

    public virtual Boolean Equals(Result? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;

        return EqualityContract == other.EqualityContract && Detail.SequenceEqual(other.Detail);
    }

    public override Int32 GetHashCode()
    {
        return HashCode.Combine(EqualityContract, Detail.Count);
    }

    private static IReadOnlyCollection<ResultDetail> CopyDetail(IReadOnlyCollection<ResultDetail>? detail)
    {
        if (detail is null)
            return [];
        if (detail.Any(t => t is null))
            throw new ArgumentException("Detail cannot contain null entries.", nameof(detail));

        return [.. detail];
    }
}

public record Result<T> : Result, IResult<T>
{
    public static Result<T> Failure(ResultDetail detail)
    {
        ArgumentNullException.ThrowIfNull(detail);

        return Failure([detail]);
    }

    public static Result<T> Failure(IReadOnlyCollection<ResultDetail> detail)
    {
        ArgumentNullException.ThrowIfNull(detail);

        var result = new Result<T>(default, detail);
        if (result.IsValid)
            throw new ArgumentException("Detail must contain at least one entry that is not a warning.", nameof(detail));

        return result;
    }

    public static Result<T> NotFound<TIdentifier>(TIdentifier identifier, String? message = null) where TIdentifier : IEquatable<TIdentifier> => Failure(ResultDetail.NotFound<T, TIdentifier>(identifier, message));

    public T? Content { get; }

    public Result(T? content, IReadOnlyCollection<ResultDetail>? detail = null) : base(detail)
    {
        Content = content;
    }

    public Boolean HasResultCode(ResultCode resultCode) => Detail.Any(t => t.Code?.Equals(resultCode) ?? false);
}
