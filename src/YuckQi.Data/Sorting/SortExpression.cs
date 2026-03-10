using System.Linq.Expressions;

namespace YuckQi.Data.Sorting;

public sealed record SortExpression<TDomainEntity>(Expression<Func<TDomainEntity, Object>> Expression, SortOrder Order)
{
    public SortExpression(Expression<Func<TDomainEntity, Object>> expression) : this(expression, SortOrder.Ascending) { }
}
