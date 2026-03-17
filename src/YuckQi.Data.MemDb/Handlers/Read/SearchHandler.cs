using System.Collections.Concurrent;
using YuckQi.Data.Filtering;
using YuckQi.Data.Handlers.Read.Abstract;
using YuckQi.Data.MemDb.Filtering.Extensions;
using YuckQi.Data.Sorting;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Domain.ValueObjects.Abstract.Interfaces;

namespace YuckQi.Data.MemDb.Handlers.Read;

public class SearchHandler<TDomainEntity, TIdentifier, TScope>(ConcurrentDictionary<TIdentifier, TDomainEntity> entities) : SearchHandlerBase<TDomainEntity, TIdentifier, TScope?> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier>
{
    protected override Int32 DoCount(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope)
    {
        var entities = GetEntities(parameters);
        var count = entities.Count();

        return count;
    }

    protected override Task<Int32> DoCount(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken)
    {
        return Task.FromResult(DoCount(parameters, scope));
    }

    protected override IReadOnlyCollection<TDomainEntity> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope)
    {
        var entities = GetEntities(parameters);
        var sorted = entities.AsQueryable().OrderBy(sort);
        var paged = sorted.Skip((page.PageNumber - 1) * page.PageSize).Take(page.PageSize);

        return [.. paged];
    }

    protected override Task<IReadOnlyCollection<TDomainEntity>> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope, CancellationToken cancellationToken)
    {
        return Task.FromResult(DoSearch(parameters, page, sort, scope));
    }

    private IEnumerable<TDomainEntity> GetEntities(IReadOnlyCollection<FilterCriteria> parameters)
    {
        return entities.Values.Where(entity => parameters.Select(t => t.ToExpression(entity)).All(t => t()));
    }
}
