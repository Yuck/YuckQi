namespace YuckQi.Data.Sorting;

public readonly struct SortCriteria(String expression, SortOrder order)
{
    public String Expression { get; } = expression ?? throw new ArgumentNullException(nameof(expression));

    public SortOrder Order { get; } = order;
}
