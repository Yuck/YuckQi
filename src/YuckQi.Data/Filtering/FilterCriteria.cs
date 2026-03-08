namespace YuckQi.Data.Filtering;

public readonly struct FilterCriteria(String fieldName, FilterOperation operation, Object? value)
{
    public String FieldName { get; } = fieldName ?? throw new ArgumentNullException(nameof(fieldName));

    public FilterOperation Operation { get; } = operation;

    public Object? Value { get; } = value;

    public FilterCriteria(String fieldName, Object? value) : this(fieldName, FilterOperation.Equal, value) { }
}
