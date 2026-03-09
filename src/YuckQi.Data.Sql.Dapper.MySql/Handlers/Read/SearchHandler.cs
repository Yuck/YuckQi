using System.Data;
using YuckQi.Data.Sql.Dapper.Abstract.Interfaces;
using YuckQi.Data.Sql.Dapper.Handlers.Read.Abstract;
using YuckQi.Data.Sql.Dapper.MySql.Internal;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.Sql.Dapper.MySql.Handlers.Read;

public class SearchHandler<TDomainEntity, TIdentifier, TScope>(ISqlGenerator sqlGenerator, IReadOnlyDictionary<Type, DbType> dbTypeMap) : SearchHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity>(sqlGenerator, dbTypeMap) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IDbTransaction?
{
    public SearchHandler() : this(new SqlGenerator<TDomainEntity>()) { }

    public SearchHandler(ISqlGenerator sqlGenerator) : this(sqlGenerator, DbTypeMap.Default) { }
}

public class SearchHandler<TDomainEntity, TIdentifier, TScope, TRecord>(ISqlGenerator sqlGenerator, IReadOnlyDictionary<Type, DbType> dbTypeMap, IMapper? mapper = null) : SearchHandlerBase<TDomainEntity, TIdentifier, TScope?, TRecord>(sqlGenerator, dbTypeMap, mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IDbTransaction?
{
    public SearchHandler(IMapper mapper) : this(new SqlGenerator<TRecord>(), mapper) { }

    public SearchHandler(ISqlGenerator sqlGenerator, IMapper mapper) : this(sqlGenerator, DbTypeMap.Default, mapper) { }
}
