using YuckQi.Data.Handlers.Write.Abstract.Interfaces;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;

namespace YuckQi.Data.Handlers.Write;

public class LogicalDeletionHandler<TDomainEntity, TIdentifier, TScope>(IRevisionHandler<TDomainEntity, TIdentifier, TScope> reviser) : LogicalDeletionHandler<TDomainEntity, TIdentifier, TScope, TDomainEntity>(reviser) where TDomainEntity : IDomainEntity<TIdentifier>, IDeletionMoment, IRevisionMoment where TIdentifier : IEquatable<TIdentifier>;

public class LogicalDeletionHandler<TDomainEntity, TIdentifier, TScope, TData>(IRevisionHandler<TDomainEntity, TIdentifier, TScope> reviser) : ILogicalDeletionHandler<TDomainEntity, TIdentifier, TScope?> where TDomainEntity : IDomainEntity<TIdentifier>, IDeletionMoment, IRevisionMoment where TIdentifier : IEquatable<TIdentifier>
{
    public TDomainEntity Delete(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (entity.DeletionMoment != null)
            return entity;

        entity.DeletionMoment = DateTime.UtcNow;

        return reviser.Revise(entity, scope);
    }

    public Task<TDomainEntity> Delete(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (entity.DeletionMoment != null)
            return Task.FromResult(entity);

        entity.DeletionMoment = DateTime.UtcNow;

        return reviser.Revise(entity, scope, cancellationToken);
    }

    public TDomainEntity Restore(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (entity.DeletionMoment == null)
            return entity;

        entity.DeletionMoment = null;

        return reviser.Revise(entity, scope);
    }

    public Task<TDomainEntity> Restore(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (entity.DeletionMoment == null)
            return Task.FromResult(entity);

        entity.DeletionMoment = null;

        return reviser.Revise(entity, scope, cancellationToken);
    }
}
