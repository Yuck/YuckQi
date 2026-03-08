using YuckQi.Data.Filtering;

namespace YuckQi.Data.Extensions;

public static class FilterCriteriaExtensions
{
    public static IReadOnlyCollection<FilterCriteria> ToFilterCollection(this Object? parameters)
    {
        return parameters switch
        {
            FilterCriteria filter => [filter],
            IEnumerable<FilterCriteria> filters => [.. filters],
            _ => parameters != null ? parameters.GetType().GetProperties().Select(t => new FilterCriteria(t.Name, FilterOperation.Equal, t.GetValue(parameters))).ToList() : []
        };
    }
}
