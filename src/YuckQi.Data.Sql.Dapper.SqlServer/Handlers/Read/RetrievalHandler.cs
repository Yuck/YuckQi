using System.Data;
using YuckQi.Data.Sql.Dapper.Abstract.Interfaces;
using YuckQi.Data.Sql.Dapper.Handlers.Read.Abstract;
using YuckQi.Data.Sql.Dapper.SqlServer.Internal;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.Sql.Dapper.SqlServer.Handlers.Read;

public class RetrievalHandler<TDomainEntity, TIdentifier, TScope>(ISqlGenerator sqlGenerator, IReadOnlyDictionary<Type, DbType> dbTypeMap) : RetrievalHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity>(sqlGenerator, dbTypeMap) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IDbTransaction?
{
    public RetrievalHandler() : this(new SqlGenerator<TDomainEntity>()) { }

    public RetrievalHandler(ISqlGenerator sqlGenerator) : this(sqlGenerator, DbTypeMap.Default) { }
}

public class RetrievalHandler<TDomainEntity, TIdentifier, TScope, TRecord>(ISqlGenerator sqlGenerator, IReadOnlyDictionary<Type, DbType> dbTypeMap, IMapper? mapper = null) : RetrievalHandlerBase<TDomainEntity, TIdentifier, TScope?, TRecord>(sqlGenerator, dbTypeMap, mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IDbTransaction?
{
    public RetrievalHandler(IMapper mapper) : this(new SqlGenerator<TRecord>(), mapper) { }

    public RetrievalHandler(ISqlGenerator sqlGenerator, IMapper mapper) : this(sqlGenerator, DbTypeMap.Default, mapper) { }
}
