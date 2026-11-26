using YuckQi.Data.Filtering;
using YuckQi.Data.Sorting;
using YuckQi.Data.Sql.Dapper.Abstract.Interfaces;
using YuckQi.Data.Sql.Dapper.Filtering;
using YuckQi.Data.Sql.Dapper.Filtering.Extensions;
using YuckQi.Data.Sql.Dapper.Schema;
using YuckQi.Domain.ValueObjects.Abstract.Interfaces;
using YuckQi.Domain.ValueObjects.Extensions;

namespace YuckQi.Data.Sql.Dapper.Oracle;

public class SqlGenerator<TRecord> : ISqlGenerator
{
    public String GenerateCountQuery(IReadOnlyCollection<FilterCriteria> parameters)
    {
        var select = "select count(*)";
        var from = BuildFromSql();
        var where = BuildWhereSql(parameters);
        var sql = $"{CombineSql(select, from, where)};";

        return sql;
    }

    public String GenerateGetQuery(IReadOnlyCollection<FilterCriteria>? parameters)
    {
        var columns = BuildColumnsSql();

        var select = $"select {columns}";
        var from = BuildFromSql();
        var where = BuildWhereSql(parameters);
        var sql = $"{CombineSql(select, from, where)};";

        return sql;
    }

    public String GenerateSearchQuery(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IReadOnlyList<SortCriteria> sort)
    {
        var columns = BuildColumnsSql();

        var select = $"select {columns}";
        var from = BuildFromSql();
        var where = BuildWhereSql(parameters);
        var order = BuildOrderSql(sort);
        var limit = $"offset {page.GetOffset()} rows fetch first {page.PageSize} rows only";
        var sql = $"{CombineSql(select, from, where, order, limit)};";

        return sql;
    }

    private static String BuildColumnSql(String fieldName)
    {
        return QuoteIdentifier(RecordSchema<TRecord>.GetColumnName(fieldName));
    }

    private static String BuildColumnsSql()
    {
        var columns = String.Join(", ", RecordSchema<TRecord>.SelectableProperties.Select(t =>
        {
            var column = RecordSchema<TRecord>.GetColumnName(t);

            return column == t.Name
                       ? QuoteIdentifier(t.Name)
                       : $"{QuoteIdentifier(column)} {QuoteIdentifier(t.Name)}";
        }));

        return columns;
    }

    private static String BuildComparison(Object? value, FilterOperation operation)
    {
        return operation switch
        {
            FilterOperation.Contains => "like",
            FilterOperation.Equal => value is not null ? "=" : "is",
            FilterOperation.GreaterThan => ">",
            FilterOperation.GreaterThanOrEqual => ">=",
            FilterOperation.LessThan => "<",
            FilterOperation.LessThanOrEqual => "<=",
            FilterOperation.NotEqual => value is not null ? "!=" : "is not",
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
    }

    private static String BuildFilterSql(FilterCriteria criteria, Int32 index)
    {
        var column = BuildColumnSql(criteria.FieldName);

        if (criteria is { Operation: FilterOperation.Contains, Value: null })
            throw new ArgumentException($"{nameof(criteria.Value)} is required for {nameof(FilterOperation.Contains)}.");

        if (criteria.Operation == FilterOperation.In)
            return BuildInSql(column, criteria.GetInValues(), index);

        var comparison = BuildComparison(criteria.Value, criteria.Operation);
        var parameter = criteria.Value is not null ? BuildParameterSql(FilterParameterNames.ForCriterion(index)) : "null";

        if (criteria.Operation == FilterOperation.Contains)
            return $"(lower({column}) {comparison} lower({parameter}) escape '\\')";

        return $"({column} {comparison} {parameter})";
    }

    private static String BuildFromSql()
    {
        var table = QuoteIdentifier(RecordSchema<TRecord>.TableName);
        var schema = RecordSchema<TRecord>.SchemaName;

        return schema is null ? $"from {table}" : $"from {QuoteIdentifier(schema)}.{table}";
    }

    private static String BuildInSql(String column, IReadOnlyList<Object?> values, Int32 index)
    {
        var parameters = values.Select((value, i) => value is not null ? BuildParameterSql(FilterParameterNames.ForElement(index, i)) : null).OfType<String>().ToArray();
        var set = parameters.Length != 0 ? String.Join(",", parameters) : "null";

        if (parameters.Length == values.Count)
            return $"({column} in ({set}))";

        if (parameters.Length == 0)
            return $"({column} is null)";

        return $"({column} in ({set}) or {column} is null)";
    }

    private static String BuildOrderSql(IEnumerable<SortCriteria> sort)
    {
        var sorting = String.Join(", ", sort.Select(t => $"{BuildColumnSql(t.Expression)}{(t.Order == SortOrder.Descending ? " desc" : String.Empty)}"));
        if (! String.IsNullOrWhiteSpace(sorting))
            return $"order by {sorting}";

        var keys = String.Join(", ", RecordSchema<TRecord>.KeyProperties.Select(t => QuoteIdentifier(RecordSchema<TRecord>.GetColumnName(t))));
        if (! String.IsNullOrWhiteSpace(keys))
            return $"order by {keys}";

        return String.Empty;
    }

    private static String BuildParameterSql(String name)
    {
        return $":{name}";
    }

    private static String BuildWhereSql(IEnumerable<FilterCriteria>? parameters)
    {
        var filter = String.Join(" and ", parameters?.Select(BuildFilterSql) ?? []);
        var where = $"{(String.IsNullOrWhiteSpace(filter) ? String.Empty : $"where {filter}")}";

        return where;
    }

    private static String CombineSql(params String[] fragments)
    {
        return String.Join(Environment.NewLine, fragments.Where(t => ! String.IsNullOrWhiteSpace(t)));
    }

    private static String QuoteIdentifier(String identifier)
    {
        return $"\"{identifier.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }
}
