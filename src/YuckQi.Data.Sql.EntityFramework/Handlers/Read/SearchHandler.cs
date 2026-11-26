using Microsoft.EntityFrameworkCore;
using YuckQi.Data.Filtering;
using YuckQi.Data.Handlers.Read.Abstract;
using YuckQi.Data.Sorting;
using YuckQi.Data.Sql.EntityFramework.Filtering.Extensions;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Domain.ValueObjects.Abstract.Interfaces;
using YuckQi.Domain.ValueObjects.Extensions;
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

    protected override IReadOnlyCollection<TDomainEntity> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IReadOnlyList<SortCriteria> sort, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var predicate = parameters.ToPredicate<TRecord>();
        var query = OrderBy(scope.Set<TRecord>().AsQueryable().Where(predicate), sort, scope);
        var records = query.Skip(page.GetOffset()).Take(page.PageSize).ToList();
        var entities = MapToEntityCollection(records);

        return entities;
    }

    protected override async Task<IReadOnlyCollection<TDomainEntity>> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IReadOnlyList<SortCriteria> sort, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var predicate = parameters.ToPredicate<TRecord>();
        var query = OrderBy(scope.Set<TRecord>().AsQueryable().Where(predicate), sort, scope);
        var records = await query.Skip(page.GetOffset()).Take(page.PageSize).ToListAsync(cancellationToken);
        var entities = MapToEntityCollection(records);

        return entities;
    }

    private static IQueryable<TRecord> OrderBy(IQueryable<TRecord> query, IReadOnlyList<SortCriteria> sort, DbContext context)
    {
        ArgumentNullException.ThrowIfNull(sort);

        if (sort.Count > 0)
            return query.OrderBy(sort);

        var key = context.Model.FindEntityType(typeof(TRecord))?.FindPrimaryKey();
        if (key is null)
            return query;

        var names = key.Properties.Select(t => t.Name).ToList();
        var ordered = query.OrderBy(t => EF.Property<Object>(t, names[0]));

        return names.Skip(1).Aggregate(ordered, (current, name) => current.ThenBy(t => EF.Property<Object>(t, name)));
    }
}
