using System.Collections;
using System.Data;
using Dapper;
using YuckQi.Data.Filtering;

namespace YuckQi.Data.Sql.Dapper.Filtering.Extensions;

public static class FilterCriteriaExtensions
{
    private const String LikeEscapeCharacter = "\\";

    public static DynamicParameters ToDynamicParameters(this IEnumerable<FilterCriteria>? parameters, IReadOnlyDictionary<Type, DbType>? dbTypeMap = null)
    {
        var result = new DynamicParameters();

        if (parameters is null)
            return result;

        foreach (var parameter in parameters)
            if (parameter.Operation == FilterOperation.In)
            {
                var set = parameter.Value is IEnumerable enumerable
                              ? enumerable.Cast<Object>().ToArray()
                              : throw new ArgumentException($"{nameof(parameter.Value)} must be convertible to {nameof(IEnumerable)}.");

                for (var i = 0; i < set.Length; i++)
                {
                    var name = $"{parameter.FieldName}{i}";
                    var value = set[i];
                    var type = value?.GetType();
                    var dbType = dbTypeMap is not null && type is not null && dbTypeMap.TryGetValue(type, out var mapped) ? (DbType?) mapped : null;

                    result.Add(name, value, dbType);
                }
            }
            else
            {
                var name = parameter.FieldName;
                var value = GetBoundValue(parameter);
                var type = value?.GetType();
                var dbType = dbTypeMap is not null && type is not null && dbTypeMap.TryGetValue(type, out var mapped) ? (DbType?) mapped : null;

                result.Add(name, value, dbType);
            }

        return result;
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
                          .Replace("_", LikeEscapeCharacter + "_", StringComparison.Ordinal)
                          .Replace("[", LikeEscapeCharacter + "[", StringComparison.Ordinal);

        return $"%{escaped}%";
    }
}
