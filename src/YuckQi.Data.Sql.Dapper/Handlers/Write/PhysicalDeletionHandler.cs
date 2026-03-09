using System.Data;
using Dapper;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.Sql.Dapper.Handlers.Write;

public class PhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope> : PhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IDbTransaction?;

public class PhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope, TRecord>(IMapper? mapper = null) : PhysicalDeletionHandlerBase<TDomainEntity, TIdentifier, TScope?, TRecord>(mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IDbTransaction?
{
    protected override Boolean DoDelete(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return scope.Connection.Delete(MapToData(entity), scope) > 0;
    }

    protected override async Task<Boolean> DoDelete(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return await scope.Connection.DeleteAsync(MapToData(entity), scope) > 0;
    }
}
