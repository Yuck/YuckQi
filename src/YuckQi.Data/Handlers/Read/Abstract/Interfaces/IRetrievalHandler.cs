using YuckQi.Data.Filtering;
using YuckQi.Domain.Entities.Abstract.Interfaces;

namespace YuckQi.Data.Handlers.Read.Abstract.Interfaces;

public interface IRetrievalHandler<TDomainEntity, in TIdentifier, in TScope> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier>
{
    TDomainEntity? Get(TIdentifier identifier, TScope? scope);

    Task<TDomainEntity?> Get(TIdentifier identifier, TScope? scope, CancellationToken cancellationToken);

    TDomainEntity? Get(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope);

    Task<TDomainEntity?> Get(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken);

    TDomainEntity? Get(Object parameters, TScope? scope);

    Task<TDomainEntity?> Get(Object parameters, TScope? scope, CancellationToken cancellationToken);

    IReadOnlyCollection<TDomainEntity> GetList(TScope? scope);

    Task<IReadOnlyCollection<TDomainEntity>> GetList(TScope? scope, CancellationToken cancellationToken);

    IReadOnlyCollection<TDomainEntity> GetList(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope);

    Task<IReadOnlyCollection<TDomainEntity>> GetList(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken);

    IReadOnlyCollection<TDomainEntity> GetList(Object parameters, TScope? scope);

    Task<IReadOnlyCollection<TDomainEntity>> GetList(Object parameters, TScope? scope, CancellationToken cancellationToken);
}
