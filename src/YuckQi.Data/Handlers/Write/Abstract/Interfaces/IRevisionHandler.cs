using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;

namespace YuckQi.Data.Handlers.Write.Abstract.Interfaces;

public interface IRevisionHandler<TDomainEntity, TIdentifier, in TScope> where TDomainEntity : IDomainEntity<TIdentifier>, IRevisionMoment where TIdentifier : IEquatable<TIdentifier>
{
    TDomainEntity Revise(TDomainEntity entity, TScope? scope);

    IEnumerable<TDomainEntity> Revise(IEnumerable<TDomainEntity> entities, TScope? scope);

    Task<TDomainEntity> Revise(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken);

    Task<IEnumerable<TDomainEntity>> Revise(IEnumerable<TDomainEntity> entities, TScope? scope, CancellationToken cancellationToken);
}
