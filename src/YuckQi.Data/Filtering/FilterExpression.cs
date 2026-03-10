using System.Linq.Expressions;

namespace YuckQi.Data.Filtering;

public sealed record FilterExpression<TDomainEntity>(Expression<Func<TDomainEntity, Object>> Expression, FilterOperation Operation, Object? Value)
{
    public FilterExpression(Expression<Func<TDomainEntity, Object>> expression, Object? value) : this(expression, FilterOperation.Equal, value) { }
}
