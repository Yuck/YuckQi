using System.Data;
using Dapper;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.Sql.Dapper.Handlers.Write;

public class CreationHandler<TDomainEntity, TIdentifier, TScope>(CreationOptions<TIdentifier>? options = null) : CreationHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity>(options, null) where TDomainEntity : IDomainEntity<TIdentifier>, ICreationMoment where TIdentifier : IEquatable<TIdentifier> where TScope : IDbTransaction?;

public class CreationHandler<TDomainEntity, TIdentifier, TScope, TRecord>(CreationOptions<TIdentifier>? options, IMapper? mapper = null) : CreationHandlerBase<TDomainEntity, TIdentifier, TScope?, TRecord>(options, mapper) where TDomainEntity : IDomainEntity<TIdentifier>, ICreationMoment where TIdentifier : IEquatable<TIdentifier> where TScope : IDbTransaction?
{
    protected override TIdentifier? DoCreate(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var record = MapToData(entity) ?? throw new InvalidOperationException();

        return scope.Connection.Insert<TIdentifier?, TRecord>(record, scope);
    }

    protected override Task<TIdentifier?> DoCreate(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var record = MapToData(entity) ?? throw new InvalidOperationException();

        return scope.Connection.InsertAsync<TIdentifier?, TRecord>(record, scope);
    }
}
