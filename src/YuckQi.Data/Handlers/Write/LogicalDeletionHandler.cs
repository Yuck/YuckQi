using YuckQi.Data.Handlers.Write.Abstract.Interfaces;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;

namespace YuckQi.Data.Handlers.Write;

public class LogicalDeletionHandler<TDomainEntity, TIdentifier, TScope>(IRevisionHandler<TDomainEntity, TIdentifier, TScope> reviser) : LogicalDeletionHandler<TDomainEntity, TIdentifier, TScope, TDomainEntity>(reviser) where TDomainEntity : IDomainEntity<TIdentifier>, IDeletionMoment, IRevisionMoment where TIdentifier : IEquatable<TIdentifier>;

public class LogicalDeletionHandler<TDomainEntity, TIdentifier, TScope, TData> : ILogicalDeletionHandler<TDomainEntity, TIdentifier, TScope?> where TDomainEntity : IDomainEntity<TIdentifier>, IDeletionMoment, IRevisionMoment where TIdentifier : IEquatable<TIdentifier>
{
    private readonly IRevisionHandler<TDomainEntity, TIdentifier, TScope> _reviser;

    public LogicalDeletionHandler(IRevisionHandler<TDomainEntity, TIdentifier, TScope> reviser)
    {
        ArgumentNullException.ThrowIfNull(reviser);

        _reviser = reviser;
    }

    public TDomainEntity Delete(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (entity.DeletionMoment is not null)
            return entity;

        return Revise(entity, DateTime.UtcNow, scope);
    }

    public Task<TDomainEntity> Delete(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (entity.DeletionMoment is not null)
            return Task.FromResult(entity);

        return Revise(entity, DateTime.UtcNow, scope, cancellationToken);
    }

    public TDomainEntity Restore(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (entity.DeletionMoment is null)
            return entity;

        return Revise(entity, null, scope);
    }

    public Task<TDomainEntity> Restore(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (entity.DeletionMoment is null)
            return Task.FromResult(entity);

        return Revise(entity, null, scope, cancellationToken);
    }

    private TDomainEntity Revise(TDomainEntity entity, DateTimeOffset? deletionMoment, TScope? scope)
    {
        var previous = entity.DeletionMoment;

        entity.DeletionMoment = deletionMoment;

        try
        {
            return _reviser.Revise(entity, scope);
        }
        catch
        {
            entity.DeletionMoment = previous;

            throw;
        }
    }

    private async Task<TDomainEntity> Revise(TDomainEntity entity, DateTimeOffset? deletionMoment, TScope? scope, CancellationToken cancellationToken)
    {
        var previous = entity.DeletionMoment;

        entity.DeletionMoment = deletionMoment;

        try
        {
            return await _reviser.Revise(entity, scope, cancellationToken);
        }
        catch
        {
            entity.DeletionMoment = previous;

            throw;
        }
    }
}
