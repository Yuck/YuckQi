using Microsoft.EntityFrameworkCore;
using YuckQi.Data.Filtering;
using YuckQi.Data.Handlers.Read.Abstract;
using YuckQi.Data.Sql.EntityFramework.Extensions;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.Sql.EntityFramework.Handlers.Read;

public class RetrievalHandler<TDomainEntity, TIdentifier, TScope> : RetrievalHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity> where TDomainEntity : class, IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : DbContext?;

public class RetrievalHandler<TDomainEntity, TIdentifier, TScope, TRecord>(IMapper? mapper = null) : RetrievalHandlerBase<TDomainEntity, TIdentifier, TScope?, TRecord>(mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : DbContext? where TRecord : class
{
    protected override TDomainEntity? DoGet(TIdentifier identifier, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var record = scope.Set<TRecord>().Find([identifier]);
        var entity = MapToEntity(record);

        return entity;
    }

    protected override async Task<TDomainEntity?> DoGet(TIdentifier identifier, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var record = await scope.Set<TRecord>().FindAsync([identifier], cancellationToken);
        var entity = MapToEntity(record);

        return entity;
    }

    protected override TDomainEntity? DoGet(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var predicate = parameters.ToPredicate<TRecord>();
        var record = scope.Set<TRecord>().AsQueryable().Where(predicate).FirstOrDefault();
        var entity = MapToEntity(record);

        return entity;
    }

    protected override async Task<TDomainEntity?> DoGet(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var predicate = parameters.ToPredicate<TRecord>();
        var record = await scope.Set<TRecord>().AsQueryable().Where(predicate).FirstOrDefaultAsync(cancellationToken);
        var entity = MapToEntity(record);

        return entity;
    }

    protected override IReadOnlyCollection<TDomainEntity> DoGetList(IReadOnlyCollection<FilterCriteria>? parameters, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var predicate = parameters.ToPredicate<TRecord>();
        var records = scope.Set<TRecord>().AsQueryable().Where(predicate).ToList();
        var entities = MapToEntityCollection(records);

        return entities;
    }

    protected override async Task<IReadOnlyCollection<TDomainEntity>> DoGetList(IReadOnlyCollection<FilterCriteria>? parameters, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var predicate = parameters.ToPredicate<TRecord>();
        var records = await scope.Set<TRecord>().AsQueryable().Where(predicate).ToListAsync(cancellationToken);
        var entities = MapToEntityCollection(records);

        return entities;
    }
}
