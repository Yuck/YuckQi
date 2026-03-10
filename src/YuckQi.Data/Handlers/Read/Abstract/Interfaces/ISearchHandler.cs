using System.Linq.Expressions;
using YuckQi.Data.Filtering;
using YuckQi.Data.Sorting;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Domain.ValueObjects.Abstract.Interfaces;

namespace YuckQi.Data.Handlers.Read.Abstract.Interfaces;

public interface ISearchHandler<TDomainEntity, TIdentifier, in TScope> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier>
{
    IPage<TDomainEntity> Search(Expression<Func<TDomainEntity, Boolean>> expression, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope);

    Task<IPage<TDomainEntity>> Search(Expression<Func<TDomainEntity, Boolean>> expression, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope, CancellationToken cancellationToken);

    IPage<TDomainEntity> Search(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope);

    Task<IPage<TDomainEntity>> Search(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope, CancellationToken cancellationToken);

    IPage<TDomainEntity> Search(Object parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope);

    Task<IPage<TDomainEntity>> Search(Object parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope, CancellationToken cancellationToken);
}
