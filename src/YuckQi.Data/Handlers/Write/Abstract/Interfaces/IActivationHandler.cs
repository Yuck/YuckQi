using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;

namespace YuckQi.Data.Handlers.Write.Abstract.Interfaces;

public interface IActivationHandler<TDomainEntity, in TIdentifier, in TScope> where TDomainEntity : IDomainEntity<TIdentifier>, IActivationMoment, IRevisionMoment where TIdentifier : IEquatable<TIdentifier>
{
    TDomainEntity Activate(TDomainEntity entity, TScope? scope);

    Task<TDomainEntity> Activate(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken);

    TDomainEntity Deactivate(TDomainEntity entity, TScope? scope);

    Task<TDomainEntity> Deactivate(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken);
}
