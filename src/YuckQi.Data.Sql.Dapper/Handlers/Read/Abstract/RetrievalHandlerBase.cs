using System.Data;
using Dapper;
using YuckQi.Data.Filtering;
using YuckQi.Data.Sql.Dapper.Abstract.Interfaces;
using YuckQi.Data.Sql.Dapper.Extensions;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.Sql.Dapper.Handlers.Read.Abstract;

public abstract class RetrievalHandlerBase<TDomainEntity, TIdentifier, TScope>(ISqlGenerator sqlGenerator, IReadOnlyDictionary<Type, DbType> dbTypeMap) : RetrievalHandlerBase<TDomainEntity, TIdentifier, TScope?, TDomainEntity>(sqlGenerator, dbTypeMap) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IDbTransaction?;

public abstract class RetrievalHandlerBase<TDomainEntity, TIdentifier, TScope, TRecord>(ISqlGenerator sqlGenerator, IReadOnlyDictionary<Type, DbType> dbTypeMap, IMapper? mapper = null) : Data.Handlers.Read.Abstract.RetrievalHandlerBase<TDomainEntity, TIdentifier, TScope?, TRecord>(mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IDbTransaction?
{
    protected override TDomainEntity? DoGet(TIdentifier identifier, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var record = scope.Connection.Get<TRecord>(identifier, scope);
        var entity = MapToEntity(record);

        return entity;
    }

    protected override async Task<TDomainEntity?> DoGet(TIdentifier identifier, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var record = await scope.Connection.GetAsync<TRecord>(identifier, scope);
        var entity = MapToEntity(record);

        return entity;
    }

    protected override TDomainEntity? DoGet(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var sql = sqlGenerator.GenerateGetQuery(parameters);
        var record = scope.Connection.QuerySingleOrDefault<TRecord>(sql, parameters.ToDynamicParameters(dbTypeMap), scope);
        var entity = MapToEntity(record);

        return entity;
    }

    protected override async Task<TDomainEntity?> DoGet(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var sql = sqlGenerator.GenerateGetQuery(parameters);
        var record = await scope.Connection.QuerySingleOrDefaultAsync<TRecord>(sql, parameters.ToDynamicParameters(dbTypeMap), scope);
        var entity = MapToEntity(record);

        return entity;
    }

    protected override IReadOnlyCollection<TDomainEntity> DoGetList(IReadOnlyCollection<FilterCriteria>? parameters, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var sql = sqlGenerator.GenerateGetQuery(parameters);
        var records = scope.Connection.Query<TRecord>(sql, parameters?.ToDynamicParameters(dbTypeMap), scope);
        var entities = MapToEntityCollection(records);

        return entities;
    }

    protected override async Task<IReadOnlyCollection<TDomainEntity>> DoGetList(IReadOnlyCollection<FilterCriteria>? parameters, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var sql = sqlGenerator.GenerateGetQuery(parameters);
        var records = await scope.Connection.QueryAsync<TRecord>(sql, parameters?.ToDynamicParameters(dbTypeMap), scope);
        var entities = MapToEntityCollection(records);

        return entities;
    }
}
