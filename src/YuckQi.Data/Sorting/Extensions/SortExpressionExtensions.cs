using System.Collections.Generic;
using System.Linq;
using YuckQi.Data.Resolvers;
using YuckQi.Data.Resolvers.Abstract.Interfaces;
using YuckQi.Data.Sorting;

namespace YuckQi.Data.Sorting.Extensions;

public static class SortExpressionExtensions
{
    public static IOrderedEnumerable<SortCriteria> ToSortCriteria<TDomainEntity, TPersistenceModel>(this IEnumerable<SortExpression<TDomainEntity>> sorts, IPropertyNameResolver? resolver = null)
    {
        var prevailing = resolver ?? new DefaultPropertyNameResolver();
        var criteria = sorts.Select(t => new SortCriteria(prevailing.Resolve<TDomainEntity, TPersistenceModel>(t.Expression), t.Order)).OrderBy(t => t.Order);

        return criteria;
    }
}
