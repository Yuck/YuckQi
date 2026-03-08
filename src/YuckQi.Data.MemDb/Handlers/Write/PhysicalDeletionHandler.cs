using System.Collections.Concurrent;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Domain.Entities.Abstract.Interfaces;

namespace YuckQi.Data.MemDb.Handlers.Write;

public class PhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope>(ConcurrentDictionary<TIdentifier, TDomainEntity> entities) : PhysicalDeletionHandlerBase<TDomainEntity, TIdentifier, TScope?> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier>
{
    protected override Boolean DoDelete(TDomainEntity entity, TScope? scope)
    {
        if (entity.Identifier == null)
            throw new InvalidOperationException();

        return entities.TryRemove(entity.Identifier, out _);
    }

    protected override Task<Boolean> DoDelete(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        return Task.FromResult(DoDelete(entity, scope));
    }
}
