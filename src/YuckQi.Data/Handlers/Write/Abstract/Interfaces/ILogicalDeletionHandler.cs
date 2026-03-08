using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;

namespace YuckQi.Data.Handlers.Write.Abstract.Interfaces;

public interface ILogicalDeletionHandler<TDomainEntity, in TIdentifier, in TScope> where TDomainEntity : IDomainEntity<TIdentifier>, IDeletionMoment, IRevisionMoment where TIdentifier : IEquatable<TIdentifier>
{
    TDomainEntity Delete(TDomainEntity entity, TScope? scope);

    Task<TDomainEntity> Delete(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken);

    TDomainEntity Restore(TDomainEntity entity, TScope? scope);

    Task<TDomainEntity> Restore(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken);
}
