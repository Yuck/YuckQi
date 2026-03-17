using Microsoft.EntityFrameworkCore;
using YuckQi.Data.Filtering;
using YuckQi.Data.Handlers.Read.Abstract;
using YuckQi.Data.Sorting;
using YuckQi.Data.Sql.EntityFramework.Filtering.Extensions;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Domain.ValueObjects.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.Sql.EntityFramework.Handlers.Read;

public class SearchHandler<TDomainEntity, TIdentifier, TScope> : SearchHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity> where TDomainEntity : class, IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : DbContext?;

public class SearchHandler<TDomainEntity, TIdentifier, TScope, TRecord>(IMapper? mapper = null) : SearchHandlerBase<TDomainEntity, TIdentifier, TScope?, TRecord>(mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : DbContext? where TRecord : class
{
    protected override Int32 DoCount(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var predicate = parameters.ToPredicate<TRecord>();
        var total = scope.Set<TRecord>().AsQueryable().Where(predicate).Count();

        return total;
    }

    protected override async Task<Int32> DoCount(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var predicate = parameters.ToPredicate<TRecord>();
        var total = await scope.Set<TRecord>().AsQueryable().Where(predicate).CountAsync(cancellationToken);

        return total;
    }

    protected override IReadOnlyCollection<TDomainEntity> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var predicate = parameters.ToPredicate<TRecord>();
        var query = scope.Set<TRecord>().AsQueryable().Where(predicate).OrderBy(sort);
        var records = query.Skip((page.PageNumber - 1) * page.PageSize).Take(page.PageSize).ToList();
        var entities = MapToEntityCollection(records);

        return entities;
    }

    protected override async Task<IReadOnlyCollection<TDomainEntity>> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var predicate = parameters.ToPredicate<TRecord>();
        var query = scope.Set<TRecord>().AsQueryable().Where(predicate).OrderBy(sort);
        var records = await query.Skip((page.PageNumber - 1) * page.PageSize).Take(page.PageSize).ToListAsync(cancellationToken);
        var entities = MapToEntityCollection(records);

        return entities;
    }
}
