using YuckQi.Data.Filtering;
using YuckQi.Data.Sorting;
using YuckQi.Data.Sql.Dapper.Abstract.Interfaces;
using YuckQi.Data.Sql.Dapper.Filtering;
using YuckQi.Data.Sql.Dapper.Filtering.Extensions;
using YuckQi.Data.Sql.Dapper.Schema;
using YuckQi.Domain.ValueObjects.Abstract.Interfaces;

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

    public String GenerateSearchQuery(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort)
    {
        var columns = BuildColumnsSql();
        var sorting = String.Join(", ", sort.Select(t => $"{BuildColumnSql(t.Expression)}{(t.Order == SortOrder.Descending ? " desc" : String.Empty)}"));

        var select = $"select {columns}";
        var from = BuildFromSql();
        var where = BuildWhereSql(parameters);
        var order = ! String.IsNullOrWhiteSpace(sorting) ? $"order by {sorting}" : String.Empty;
        var limit = $"offset {(page.PageNumber - 1) * page.PageSize} rows fetch first {page.PageSize} rows only";
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
            FilterOperation.In => "in",
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

        var comparison = BuildComparison(criteria.Value, criteria.Operation);

        if (criteria.Operation == FilterOperation.In)
        {
            var values = criteria.GetInValues();
            var set = values.Count != 0 ? String.Join(",", values.Select((_, i) => BuildParameterSql(FilterParameterNames.ForElement(index, i)))) : "null";

            return $"({column} {comparison} ({set}))";
        }

        var parameter = criteria.Value is not null ? BuildParameterSql(FilterParameterNames.ForCriterion(index)) : "null";

        if (criteria.Operation == FilterOperation.Contains)
            return $"({column} {comparison} {parameter} escape '\\')";

        return $"({column} {comparison} {parameter})";
    }

    private static String BuildFromSql()
    {
        var table = QuoteIdentifier(RecordSchema<TRecord>.TableName);
        var schema = RecordSchema<TRecord>.SchemaName;

        return schema is null ? $"from {table}" : $"from {QuoteIdentifier(schema)}.{table}";
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
