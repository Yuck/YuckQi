using YuckQi.Data.Handlers.Write.Abstract.Interfaces;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;

namespace YuckQi.Data.Handlers.Write;

public class ActivationHandler<TDomainEntity, TIdentifier, TScope>(IRevisionHandler<TDomainEntity, TIdentifier, TScope?> reviser) : ActivationHandler<TDomainEntity, TIdentifier, TScope, TDomainEntity>(reviser) where TDomainEntity : IDomainEntity<TIdentifier>, IActivationMoment, IRevisionMoment where TIdentifier : IEquatable<TIdentifier>;

public class ActivationHandler<TDomainEntity, TIdentifier, TScope, TData> : IActivationHandler<TDomainEntity, TIdentifier, TScope?> where TDomainEntity : IDomainEntity<TIdentifier>, IActivationMoment, IRevisionMoment where TIdentifier : IEquatable<TIdentifier>
{
    private readonly IRevisionHandler<TDomainEntity, TIdentifier, TScope?> _reviser;

    public ActivationHandler(IRevisionHandler<TDomainEntity, TIdentifier, TScope?> reviser)
    {
        ArgumentNullException.ThrowIfNull(reviser);

        _reviser = reviser;
    }

    public TDomainEntity Activate(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (entity.ActivationMoment is not null)
            return entity;

        entity.ActivationMoment = DateTime.UtcNow;

        return _reviser.Revise(entity, scope);
    }

    public Task<TDomainEntity> Activate(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (entity.ActivationMoment is not null)
            return Task.FromResult(entity);

        entity.ActivationMoment = DateTime.UtcNow;

        return _reviser.Revise(entity, scope, cancellationToken);
    }

    public TDomainEntity Deactivate(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (entity.ActivationMoment is null)
            return entity;

        entity.ActivationMoment = null;

        return _reviser.Revise(entity, scope);
    }

    public Task<TDomainEntity> Deactivate(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (entity.ActivationMoment is null)
            return Task.FromResult(entity);

        entity.ActivationMoment = null;

        return _reviser.Revise(entity, scope, cancellationToken);
    }
}
