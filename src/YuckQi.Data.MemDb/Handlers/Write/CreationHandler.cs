using System.Collections.Concurrent;
using YuckQi.Data.Exceptions;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;

namespace YuckQi.Data.MemDb.Handlers.Write;

public class CreationHandler<TDomainEntity, TIdentifier, TScope>(ConcurrentDictionary<TIdentifier, TDomainEntity> entities, CreationOptions<TIdentifier>? options = null) : CreationHandlerBase<TDomainEntity, TIdentifier, TScope?>(options) where TDomainEntity : IDomainEntity<TIdentifier>, ICreationMoment where TIdentifier : IEquatable<TIdentifier>
{
    protected override TIdentifier? DoCreate(TDomainEntity entity, TScope? scope)
    {
        if (entity.Identifier is null)
            throw new InvalidOperationException("Entity identifier must not be null.");

        return entities.TryAdd(entity.Identifier, entity) ? entity.Identifier : throw new CreationException<TDomainEntity>();
    }

    protected override Task<TIdentifier?> DoCreate(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        return Task.FromResult(DoCreate(entity, scope));
    }
}
