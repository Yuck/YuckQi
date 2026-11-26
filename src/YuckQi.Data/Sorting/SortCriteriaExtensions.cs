using System.Linq.Expressions;

namespace YuckQi.Data.Sorting;

public static class SortCriteriaExtensions
{
    public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, IReadOnlyList<SortCriteria> sort)
    {
        ArgumentNullException.ThrowIfNull(sort);

        if (sort.Count == 0)
            return source;

        var first = sort[0];
        var ordered = first.Order == SortOrder.Descending ? source.OrderByDescending(first.Expression) : source.OrderBy(first.Expression);

        return sort.Skip(1).Aggregate(ordered, (current, item) => item.Order == SortOrder.Descending ? current.ThenByDescending(item.Expression) : current.ThenBy(item.Expression));
    }

    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, String fieldName)
    {
        return source.OrderBy(ToLambda(source, fieldName));
    }

    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, String fieldName)
    {
        return source.OrderByDescending(ToLambda(source, fieldName));
    }

    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, String fieldName)
    {
        return source.ThenBy(ToLambda(source, fieldName));
    }

    public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, String fieldName)
    {
        return source.ThenByDescending(ToLambda(source, fieldName));
    }

    private static Boolean CanBeNull(Type type)
    {
        if (! type.IsValueType)
            return true;

        return Nullable.GetUnderlyingType(type) is not null;
    }

    private static Expression<Func<T, Object>> ToLambda<T>(IQueryable<T> source, String fieldName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);

        var parameter = Expression.Parameter(typeof(T));
        var path = fieldName.Split('.');
        var value = source.Provider is EnumerableQuery ? ToNullSafeValue(parameter, path, 0) : ToValue(parameter, path);

        return Expression.Lambda<Func<T, Object>>(value, parameter);
    }

    private static Expression ToNullSafeValue(Expression instance, IReadOnlyList<String> path, Int32 index)
    {
        if (index == path.Count)
            return Expression.Convert(instance, typeof(Object));

        var value = ToNullSafeValue(Expression.PropertyOrField(instance, path[index]), path, index + 1);
        if (instance is ParameterExpression || ! CanBeNull(instance.Type))
            return value;

        return Expression.Condition(Expression.Equal(instance, Expression.Constant(null, instance.Type)), Expression.Constant(null, typeof(Object)), value);
    }

    private static Expression ToValue(Expression parameter, IEnumerable<String> path)
    {
        var member = path.Aggregate(parameter, Expression.PropertyOrField);

        return Expression.Convert(member, typeof(Object));
    }
}
