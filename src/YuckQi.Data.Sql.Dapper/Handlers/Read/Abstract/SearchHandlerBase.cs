using System.Data;
using Dapper;
using YuckQi.Data.Filtering;
using YuckQi.Data.Sorting;
using YuckQi.Data.Sql.Dapper.Abstract.Interfaces;
using YuckQi.Data.Sql.Dapper.Extensions;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Domain.ValueObjects.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.Sql.Dapper.Handlers.Read.Abstract;

public abstract class SearchHandlerBase<TDomainEntity, TIdentifier, TScope>(ISqlGenerator sqlGenerator, IReadOnlyDictionary<Type, DbType> dbTypeMap) : SearchHandlerBase<TDomainEntity, TIdentifier, TScope?, TDomainEntity>(sqlGenerator, dbTypeMap) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IDbTransaction?;

public abstract class SearchHandlerBase<TDomainEntity, TIdentifier, TScope, TRecord>(ISqlGenerator sqlGenerator, IReadOnlyDictionary<Type, DbType> dbTypeMap, IMapper? mapper = null) : Data.Handlers.Read.Abstract.SearchHandlerBase<TDomainEntity, TIdentifier, TScope?, TRecord>(mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IDbTransaction?
{
    protected override Int32 DoCount(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var sql = sqlGenerator.GenerateCountQuery(parameters);
        var total = scope.Connection.ExecuteScalar<Int32>(sql, parameters.ToDynamicParameters(dbTypeMap), scope);

        return total;
    }

    protected override Task<Int32> DoCount(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var sql = sqlGenerator.GenerateCountQuery(parameters);
        var total = scope.Connection.ExecuteScalarAsync<Int32>(sql, parameters.ToDynamicParameters(dbTypeMap), scope);

        return total;
    }

    protected override IReadOnlyCollection<TDomainEntity> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var sql = sqlGenerator.GenerateSearchQuery(parameters, page, sort);
        var records = scope.Connection.Query<TRecord>(sql, parameters.ToDynamicParameters(dbTypeMap), scope);
        var entities = MapToEntityCollection(records);

        return entities;
    }

    protected override async Task<IReadOnlyCollection<TDomainEntity>> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var sql = sqlGenerator.GenerateSearchQuery(parameters, page, sort);
        var records = await scope.Connection.QueryAsync<TRecord>(sql, parameters.ToDynamicParameters(dbTypeMap), scope);
        var entities = MapToEntityCollection(records);

        return entities;
    }
}
