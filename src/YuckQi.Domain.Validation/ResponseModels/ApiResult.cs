namespace YuckQi.Domain.Validation.ResponseModels;

public record ApiResult
{
    public IReadOnlyCollection<ApiResultDetail> Detail { get; set; } = [];

    public virtual Boolean Equals(ApiResult? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;
        if (EqualityContract != other.EqualityContract)
            return false;
        if (Detail is null || other.Detail is null)
            return ReferenceEquals(Detail, other.Detail);

        return Detail.SequenceEqual(other.Detail);
    }

    public override Int32 GetHashCode()
    {
        if (Detail is null)
            return HashCode.Combine(EqualityContract);

        return HashCode.Combine(EqualityContract, Detail.Count);
    }
}

public record ApiResult<T> : ApiResult
{
    public T? Content { get; set; }
}
