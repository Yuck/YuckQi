using YuckQi.Data.Resolvers;
using YuckQi.Data.Resolvers.Abstract.Interfaces;

namespace YuckQi.Data.Sorting.Extensions;

public static class SortExpressionExtensions
{
    public static IReadOnlyList<SortCriteria> ToSortCriteria<TDomainEntity, TPersistenceModel>(this IEnumerable<SortExpression<TDomainEntity>> sorts, IPropertyNameResolver? resolver = null)
    {
        var prevailing = resolver ?? new DefaultPropertyNameResolver();
        var criteria = sorts.Select(t => new SortCriteria(prevailing.Resolve<TDomainEntity, TPersistenceModel>(t.Expression), t.Order)).ToList();

        return criteria;
    }
}
