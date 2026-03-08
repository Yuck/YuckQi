using YuckQi.Data.Extensions;
using YuckQi.Data.Filtering;
using YuckQi.Data.Handlers.Read.Abstract.Interfaces;
using YuckQi.Data.Sorting;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Domain.ValueObjects;
using YuckQi.Domain.ValueObjects.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.Handlers.Read.Abstract;

public abstract class SearchHandlerBase<TDomainEntity, TIdentifier, TScope>(IMapper? mapper = null) : SearchHandlerBase<TDomainEntity, TIdentifier, TScope, TDomainEntity>(mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier>;

public abstract class SearchHandlerBase<TDomainEntity, TIdentifier, TScope, TData>(IMapper? mapper = null) : HandlerBase<TDomainEntity, TData>(mapper), ISearchHandler<TDomainEntity, TIdentifier, TScope?> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier>
{
    public IPage<TDomainEntity> Search(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var entities = DoSearch(parameters, page, sort, scope);
        var total = DoCount(parameters, scope);

        return new Page<TDomainEntity>(entities, total, page.PageNumber, page.PageSize);
    }

    public async Task<IPage<TDomainEntity>> Search(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var entities = await DoSearch(parameters, page, sort, scope, cancellationToken);
        var total = await DoCount(parameters, scope, cancellationToken);

        return new Page<TDomainEntity>(entities, total, page.PageNumber, page.PageSize);
    }

    public IPage<TDomainEntity> Search(Object parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope)
    {
        return Search(parameters.ToFilterCollection(), page, sort, scope);
    }

    public Task<IPage<TDomainEntity>> Search(Object parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope, CancellationToken cancellationToken)
    {
        return Search(parameters.ToFilterCollection(), page, sort, scope, cancellationToken);
    }

    protected abstract Int32 DoCount(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope);

    protected abstract Task<Int32> DoCount(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken);

    protected abstract IReadOnlyCollection<TDomainEntity> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope);

    protected abstract Task<IReadOnlyCollection<TDomainEntity>> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope, CancellationToken cancellationToken);
}
