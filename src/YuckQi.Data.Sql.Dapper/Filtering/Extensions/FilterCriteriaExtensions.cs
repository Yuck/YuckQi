using System.Collections;
using System.Data;
using Dapper;
using YuckQi.Data.Filtering;

namespace YuckQi.Data.Sql.Dapper.Filtering.Extensions;

public static class FilterCriteriaExtensions
{
    private const String LikeEscapeCharacter = "\\";

    public static IReadOnlyList<Object?> GetInValues(this FilterCriteria parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        if (parameter.Value is String or Byte[])
            throw new ArgumentException($"{nameof(parameter.Value)} for {nameof(FilterOperation.In)} on '{parameter.FieldName}' must be a collection of values, not a single {parameter.Value.GetType().Name}.");

        if (parameter.Value is not IEnumerable enumerable)
            throw new ArgumentException($"{nameof(parameter.Value)} must be convertible to {nameof(IEnumerable)}.");

        return enumerable.Cast<Object?>().ToArray();
    }

    public static DynamicParameters ToDynamicParameters(this IEnumerable<FilterCriteria>? parameters, IReadOnlyDictionary<Type, DbType>? dbTypeMap = null)
    {
        var result = new DynamicParameters();

        if (parameters is null)
            return result;

        var index = 0;

        foreach (var parameter in parameters)
        {
            if (parameter.Operation == FilterOperation.In)
            {
                var values = parameter.GetInValues();

                for (var i = 0; i < values.Count; i++)
                {
                    if (values[i] is null)
                        continue;

                    AddParameter(result, FilterParameterNames.ForElement(index, i), values[i], dbTypeMap);
                }
            }
            else
            {
                AddParameter(result, FilterParameterNames.ForCriterion(index), GetBoundValue(parameter), dbTypeMap);
            }

            index++;
        }

        return result;
    }

    private static void AddParameter(DynamicParameters parameters, String name, Object? value, IReadOnlyDictionary<Type, DbType>? dbTypeMap)
    {
        var type = value?.GetType();
        var dbType = dbTypeMap is not null && type is not null && dbTypeMap.TryGetValue(type, out var mapped) ? (DbType?) mapped : null;

        parameters.Add(name, value, dbType);
    }

    private static Object? GetBoundValue(FilterCriteria parameter)
    {
        if (parameter.Operation != FilterOperation.Contains)
            return parameter.Value;

        if (parameter.Value is null)
            throw new ArgumentException($"{nameof(parameter.Value)} is required for {nameof(FilterOperation.Contains)}.");

        var text = Convert.ToString(parameter.Value) ?? String.Empty;
        var escaped = text.Replace(LikeEscapeCharacter, LikeEscapeCharacter + LikeEscapeCharacter, StringComparison.Ordinal)
                          .Replace("%", LikeEscapeCharacter + "%", StringComparison.Ordinal)
                          .Replace("_", LikeEscapeCharacter + "_", StringComparison.Ordinal);

        return $"%{escaped}%";
    }
}
