using System.Collections.Generic;
using System.Linq;
using YuckQi.Data.Filtering;
using YuckQi.Data.Resolvers;
using YuckQi.Data.Resolvers.Abstract.Interfaces;

namespace YuckQi.Data.Filtering.Extensions;

public static class FilterExpressionExtensions
{
    public static IReadOnlyCollection<FilterCriteria> ToFilterCriteria<TDomainEntity, TPersistenceModel>(this IEnumerable<FilterExpression<TDomainEntity>> filters, IPropertyNameResolver? resolver = null)
    {
        var prevailing = resolver ?? new DefaultPropertyNameResolver();
        var criteria = filters.Select(t => new FilterCriteria(prevailing.Resolve<TDomainEntity, TPersistenceModel>(t.Expression), t.Operation, t.Value)).ToList();

        return criteria;
    }
}
