using System.Reflection;

namespace YuckQi.Data.Filtering.Extensions;

public static class FilterCriteriaExtensions
{
    public static IReadOnlyCollection<FilterCriteria> ToFilterCollection(this Object? parameters)
    {
        return parameters switch
        {
            null => [],
            FilterCriteria filter => [filter],
            IReadOnlyCollection<FilterCriteria> filters => filters,
            IEnumerable<FilterCriteria> filters => [.. filters],
            _ => [.. parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(t => t.CanRead).Select(t => new FilterCriteria(t.Name, t.GetValue(parameters)))]
        };
    }
}
