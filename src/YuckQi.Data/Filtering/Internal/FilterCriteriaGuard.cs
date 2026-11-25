namespace YuckQi.Data.Filtering.Internal;

internal static class FilterCriteriaGuard
{
    public static void ThrowIfInvalid(IReadOnlyCollection<FilterCriteria> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var index = 0;

        foreach (var parameter in parameters)
        {
            if (parameter is null)
                throw new ArgumentException($"The filter at index {index} is null.", nameof(parameters));

            if (parameter.Operation == FilterOperation.Unknown)
                throw new ArgumentException($"The filter on '{parameter.FieldName}' does not specify a {nameof(FilterOperation)}.", nameof(parameters));

            index++;
        }
    }

    public static void ThrowIfEmptyOrInvalid(IReadOnlyCollection<FilterCriteria> parameters)
    {
        ThrowIfInvalid(parameters);

        if (parameters.Count == 0)
            throw new ArgumentException("At least one filter is required. To get every entity, call GetList with only a scope.", nameof(parameters));
    }
}
