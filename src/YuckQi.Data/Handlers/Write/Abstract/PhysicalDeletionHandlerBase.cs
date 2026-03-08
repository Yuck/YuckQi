using YuckQi.Data.Exceptions;
using YuckQi.Data.Handlers.Write.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.Handlers.Write.Abstract;

public abstract class PhysicalDeletionHandlerBase<TDomainEntity, TIdentifier, TScope>(IMapper? mapper = null) : PhysicalDeletionHandlerBase<TDomainEntity, TIdentifier, TScope, TDomainEntity>(mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier>;

public abstract class PhysicalDeletionHandlerBase<TDomainEntity, TIdentifier, TScope, TData>(IMapper? mapper = null) : HandlerBase<TDomainEntity, TData>(mapper), IPhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope?> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier>
{
    public TDomainEntity Delete(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return DoDelete(entity, scope) ? entity : throw new PhysicalDeletionException<TDomainEntity, TIdentifier>(entity.Identifier);
    }

    public async Task<TDomainEntity> Delete(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return await DoDelete(entity, scope, cancellationToken) ? entity : throw new PhysicalDeletionException<TDomainEntity, TIdentifier>(entity.Identifier);
    }

    protected abstract Boolean DoDelete(TDomainEntity entity, TScope? scope);

    protected abstract Task<Boolean> DoDelete(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken);
}
