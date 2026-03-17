namespace YuckQi.Data.Filtering;

public sealed record FilterCriteria(String FieldName, FilterOperation Operation, Object? Value)
{
    public FilterCriteria(String fieldName, Object? value) : this(fieldName, FilterOperation.Equal, value) { }
}
