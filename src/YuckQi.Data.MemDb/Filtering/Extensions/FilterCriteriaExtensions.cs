using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using YuckQi.Data.Filtering;

namespace YuckQi.Data.MemDb.Filtering.Extensions;

internal static class FilterCriteriaExtensions
{
    public static Func<Boolean> ToExpression<T>(this FilterCriteria criteria, T instance)
    {
        var property = Expression.Property(Expression.Constant(instance), criteria.FieldName);
        var value = Expression.Constant(criteria.Value);
        var body = criteria.Operation switch
        {
            FilterOperation.Contains => ToContainsExpression(property, criteria.Value),
            FilterOperation.Equal => Expression.Equal(property, value) as Expression,
            FilterOperation.GreaterThan => Expression.GreaterThan(property, value),
            FilterOperation.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, value),
            FilterOperation.In => Expression.Call(GetContainsMethodInfo(property), Expression.Constant(criteria.Value as IEnumerable), property),
            FilterOperation.LessThan => Expression.LessThan(property, value),
            FilterOperation.LessThanOrEqual => Expression.LessThanOrEqual(property, value),
            FilterOperation.NotEqual => Expression.NotEqual(property, value),
            _ => throw new NotSupportedException()
        };

        return Expression.Lambda<Func<Boolean>>(body).Compile();
    }

    private static MethodInfo GetContainsMethodInfo(MemberExpression property)
    {
        var methods = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public);
        var contains = methods.Single(t => t.Name == "Contains" && t.GetParameters().Length == 2);
        var generic = contains.MakeGenericMethod(property.Type);

        return generic;
    }

    private static Expression ToContainsExpression(MemberExpression property, Object? value)
    {
        if (value is null)
            throw new ArgumentException($"{nameof(value)} is required for {nameof(FilterOperation.Contains)}.");

        var text = Convert.ToString(value) ?? String.Empty;
        var method = typeof(String).GetMethod(nameof(String.Contains), [typeof(String), typeof(StringComparison)]) ?? throw new InvalidOperationException();
        var instance = property.Type == typeof(String)
                           ? (Expression) Expression.Coalesce(property, Expression.Constant(String.Empty))
                           : Expression.Call(property, nameof(ToString), Type.EmptyTypes);

        return Expression.Call(instance, method, Expression.Constant(text), Expression.Constant(StringComparison.OrdinalIgnoreCase));
    }
}
