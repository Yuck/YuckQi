using Microsoft.EntityFrameworkCore;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.Sql.EntityFramework.Handlers.Write;

public class PhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope> : PhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity> where TDomainEntity : class, IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : DbContext?;

public class PhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope, TRecord>(IMapper? mapper = null) : PhysicalDeletionHandlerBase<TDomainEntity, TIdentifier, TScope?, TRecord>(mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : DbContext? where TRecord : class
{
    protected override Boolean DoDelete(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var record = MapToData(entity);

        if (record == null)
            return false;

        scope.Set<TRecord>().Remove(record);

        return true;
    }

    protected override async Task<Boolean> DoDelete(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var record = MapToData(entity);

        if (record == null)
            return false;

        scope.Set<TRecord>().Remove(record);

        return true;
    }
}
