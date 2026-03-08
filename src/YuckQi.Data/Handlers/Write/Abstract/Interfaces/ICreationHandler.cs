using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;

namespace YuckQi.Data.Handlers.Write.Abstract.Interfaces;

public interface ICreationHandler<TDomainEntity, TIdentifier, in TScope> where TDomainEntity : IDomainEntity<TIdentifier>, ICreationMoment where TIdentifier : IEquatable<TIdentifier>
{
    TDomainEntity Create(TDomainEntity entity, TScope? scope);

    IEnumerable<TDomainEntity> Create(IEnumerable<TDomainEntity> entities, TScope? scope);

    Task<TDomainEntity> Create(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken);

    Task<IEnumerable<TDomainEntity>> Create(IEnumerable<TDomainEntity> entities, TScope? scope, CancellationToken cancellationToken);
}
