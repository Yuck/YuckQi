using YuckQi.Domain.Entities.Abstract.Interfaces;

namespace YuckQi.Data.Handlers.Write.Abstract.Interfaces;

public interface IPhysicalDeletionHandler<TDomainEntity, in TIdentifier, in TScope> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier>
{
    TDomainEntity Delete(TDomainEntity entity, TScope? scope);

    Task<TDomainEntity> Delete(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken);
}
