using Microsoft.EntityFrameworkCore;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.Sql.EntityFramework.Handlers.Write;

public class RevisionHandler<TDomainEntity, TIdentifier, TScope>(RevisionOptions? options) : RevisionHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity>(options, null) where TDomainEntity : class, IDomainEntity<TIdentifier>, IRevisionMoment where TIdentifier : IEquatable<TIdentifier> where TScope : DbContext?;

public class RevisionHandler<TDomainEntity, TIdentifier, TScope, TRecord>(RevisionOptions? options = null, IMapper? mapper = null) : RevisionHandlerBase<TDomainEntity, TIdentifier, TScope?, TRecord>(options, mapper) where TDomainEntity : IDomainEntity<TIdentifier>, IRevisionMoment where TIdentifier : IEquatable<TIdentifier> where TScope : DbContext? where TRecord : class
{
    protected override Boolean DoRevise(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var record = MapToData(entity);

        if (record is null)
            return false;

        scope.Set<TRecord>().Update(record);

        return true;
    }

    protected override async Task<Boolean> DoRevise(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var record = MapToData(entity);

        if (record is null)
            return false;

        scope.Set<TRecord>().Update(record);

        return true;
    }
}
