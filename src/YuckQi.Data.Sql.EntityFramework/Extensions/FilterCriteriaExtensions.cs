using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using YuckQi.Data.Filtering;

namespace YuckQi.Data.Sql.EntityFramework.Extensions;

internal static class FilterCriteriaExtensions
{
    public static Expression<Func<T, Boolean>> ToPredicate<T>(this IReadOnlyCollection<FilterCriteria>? criteria)
    {
        if (criteria is null || criteria.Count == 0)
            return _ => true;

        var parameter = Expression.Parameter(typeof(T));
        var body = criteria.Select(t => ToExpression(t, parameter))
                           .Aggregate((Expression?) null, (t, u) => t is null ? u : Expression.AndAlso(t, u));

        return body is not null ? Expression.Lambda<Func<T, Boolean>>(body, parameter) : (_ => true);
    }

    private static Expression ToExpression(FilterCriteria criteria, ParameterExpression parameter)
    {
        var property = GetPropertyExpression(parameter, criteria.FieldName);
        var value = criteria.Value;
        var type = property.Type;

        if (value is not null && value.GetType() != type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;

            if (underlying.IsInstanceOfType(value))
                value = Convert.ChangeType(value, underlying);
        }

        var constant = value is not null ? Expression.Constant(value, type) : Expression.Constant(null, type);

        return criteria.Operation switch
        {
            FilterOperation.Equal => Expression.Equal(property, constant),
            FilterOperation.NotEqual => Expression.NotEqual(property, constant),
            FilterOperation.GreaterThan => Expression.GreaterThan(property, constant),
            FilterOperation.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, constant),
            FilterOperation.LessThan => Expression.LessThan(property, constant),
            FilterOperation.LessThanOrEqual => Expression.LessThanOrEqual(property, constant),
            FilterOperation.In => ToInExpression(property, value),
            _ => throw new NotSupportedException()
        };
    }

    private static MemberExpression GetPropertyExpression(Expression parameter, String path)
    {
        var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
        var result = path.Split('.')
                         .Aggregate(parameter, (t, u) =>
                         {
                             var type = t.Type;
                             var property = type.GetProperty(u, flags) ?? throw new InvalidOperationException($"Property '{u}' not found on type '{type.Name}'.");

                             return Expression.Property(t, property);
                         });

        return (MemberExpression) result;
    }

    private static Expression ToInExpression(MemberExpression property, Object? value)
    {
        if (value is not IEnumerable enumerable)
            return Expression.Equal(property, Expression.Constant(value, property.Type));

        var type = property.Type;
        var listType = typeof(List<>).MakeGenericType(type);
        var list = enumerable.Cast<Object>()
                             .Select(t => Convert.ChangeType(t, type))
                             .Aggregate((IList) (Activator.CreateInstance(listType) ?? throw new InvalidOperationException()), (t, u) =>
                             {
                                t.Add(u);

                                return t;
                            });

        var contains = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                         .Single(t => t.Name == "Contains" && t.GetParameters().Length == 2);
        var genericContains = contains.MakeGenericMethod(type);
        var collection = Expression.Constant(list);

        return Expression.Call(genericContains, collection, property);
    }
}
