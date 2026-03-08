using System.Collections.Concurrent;
using YuckQi.Data.Exceptions;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;

namespace YuckQi.Data.MemDb.Handlers.Write;

public class RevisionHandler<TDomainEntity, TIdentifier, TScope>(ConcurrentDictionary<TIdentifier, TDomainEntity> entities, RevisionOptions? options = null) : RevisionHandlerBase<TDomainEntity, TIdentifier, TScope?>(options) where TDomainEntity : IDomainEntity<TIdentifier>, IRevisionMoment where TIdentifier : IEquatable<TIdentifier>
{
    protected override Boolean DoRevise(TDomainEntity entity, TScope? scope)
    {
        if (entity.Identifier == null)
            throw new InvalidOperationException();

        return entities.TryUpdate(entity.Identifier, entity, entities.TryGetValue(entity.Identifier, out var current) ? current : throw new RevisionException<TDomainEntity, TIdentifier>(entity.Identifier));
    }

    protected override Task<Boolean> DoRevise(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        return Task.FromResult(DoRevise(entity, scope));
    }
}
