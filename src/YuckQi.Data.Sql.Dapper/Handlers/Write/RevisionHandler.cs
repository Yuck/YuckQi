using System.Data;
using Dapper;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Data.Sql.Dapper.Internal;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.Sql.Dapper.Handlers.Write;

public class RevisionHandler<TDomainEntity, TIdentifier, TScope>(RevisionOptions? options) : RevisionHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity>(options, null) where TDomainEntity : IDomainEntity<TIdentifier>, IRevisionMoment where TIdentifier : IEquatable<TIdentifier> where TScope : IDbTransaction?;

public class RevisionHandler<TDomainEntity, TIdentifier, TScope, TRecord>(RevisionOptions? options = null, IMapper? mapper = null) : RevisionHandlerBase<TDomainEntity, TIdentifier, TScope?, TRecord>(options, mapper) where TDomainEntity : IDomainEntity<TIdentifier>, IRevisionMoment where TIdentifier : IEquatable<TIdentifier> where TScope : IDbTransaction?
{
    protected override Boolean DoRevise(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var record = MapToData(entity) ?? throw new InvalidOperationException();

        SimpleCrudScaffold<TRecord>.IncludeOmittedUpdateColumns(record);

        return scope.Connection.Update(record, scope) > 0;
    }

    protected override async Task<Boolean> DoRevise(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var record = MapToData(entity) ?? throw new InvalidOperationException();

        SimpleCrudScaffold<TRecord>.IncludeOmittedUpdateColumns(record);

        return await scope.Connection.UpdateAsync(record, scope, token: cancellationToken) > 0;
    }
}
