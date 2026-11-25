using System.Collections;
using System.Reflection;

namespace YuckQi.Data.Filtering.Extensions;

public static class FilterCriteriaExtensions
{
    public static IReadOnlyCollection<FilterCriteria> ToFilterCollection(this Object? parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var filters = parameters switch
        {
            FilterCriteria filter => [filter],
            IReadOnlyCollection<FilterCriteria> collection => collection,
            IEnumerable<FilterCriteria> sequence => [.. sequence],
            IEnumerable<KeyValuePair<String, Object?>> pairs => [.. pairs.Select(t => new FilterCriteria(t.Key, t.Value))],
            IDictionary dictionary => FromDictionary(dictionary),
            _ => FromProperties(parameters)
        };

        if (filters.Count == 0)
            throw new ArgumentException($"The filter parameters of type '{parameters.GetType().Name}' contain no filters. To get every entity, call GetList with only a scope, or pass an empty {nameof(FilterCriteria)} collection to Search.", nameof(parameters));

        return filters;
    }

    private static IReadOnlyCollection<FilterCriteria> FromDictionary(IDictionary parameters)
    {
        var filters = new List<FilterCriteria>(parameters.Count);

        foreach (DictionaryEntry entry in parameters)
        {
            if (entry.Key is not String fieldName)
                throw new ArgumentException($"Dictionary keys must be field names of type {nameof(String)}, but '{entry.Key}' is of type '{entry.Key.GetType().Name}'.", nameof(parameters));

            filters.Add(new FilterCriteria(fieldName, entry.Value));
        }

        return filters;
    }

    private static IReadOnlyCollection<FilterCriteria> FromProperties(Object parameters)
    {
        var type = parameters.GetType();

        if (IsValueOrFrameworkType(type) || parameters is IEnumerable)
            throw new ArgumentException($"A value of type '{type.Name}' cannot be used as filter parameters. Pass an object whose properties name the fields to filter on, a dictionary of field names to values, or {nameof(FilterCriteria)}. To get a single entity by identifier, call Get with a value of the handler's identifier type.", nameof(parameters));

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(t => t.GetGetMethod() is not null && t.GetIndexParameters().Length == 0).ToList();

        if (properties.Count == 0)
            throw new ArgumentException($"Type '{type.Name}' has no public readable properties to filter on.", nameof(parameters));

        return [.. properties.Select(t => new FilterCriteria(t.Name, t.GetValue(parameters)))];
    }

    private static Boolean IsValueOrFrameworkType(Type type)
    {
        if (type.IsPrimitive || type.IsEnum)
            return true;

        var name = type.Namespace;

        if (name is null)
            return false;

        return name == nameof(System) || name.StartsWith($"{nameof(System)}.", StringComparison.Ordinal);
    }
}
