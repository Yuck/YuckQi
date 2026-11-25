namespace YuckQi.Data.Sql.Dapper.Filtering;

public static class FilterParameterNames
{
    public static String ForCriterion(Int32 criterionIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(criterionIndex);

        return $"p{criterionIndex}";
    }

    public static String ForElement(Int32 criterionIndex, Int32 elementIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(criterionIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(elementIndex);

        return $"p{criterionIndex}_{elementIndex}";
    }
}
