namespace YuckQi.Application.Core.Behaviors.Caching;

public readonly record struct CacheKey
{
    private readonly String _value;

    public CacheKey(String value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        _value = value;
    }

    public static implicit operator String(CacheKey key)
    {
        return key._value;
    }

    public static implicit operator CacheKey(String value)
    {
        return new CacheKey(value);
    }

    public override String ToString()
    {
        return _value;
    }
}
