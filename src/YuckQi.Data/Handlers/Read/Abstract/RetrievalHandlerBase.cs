using YuckQi.Data.Extensions;
using YuckQi.Data.Filtering;
using YuckQi.Data.Handlers.Read.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.Handlers.Read.Abstract;

public abstract class RetrievalHandlerBase<TDomainEntity, TIdentifier, TScope>(IMapper? mapper = null) : RetrievalHandlerBase<TDomainEntity, TIdentifier, TScope, TDomainEntity>(mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier>;

public abstract class RetrievalHandlerBase<TDomainEntity, TIdentifier, TScope, TData>(IMapper? mapper = null) : HandlerBase<TDomainEntity, TData>(mapper), IRetrievalHandler<TDomainEntity, TIdentifier, TScope?> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier>
{
    public TDomainEntity? Get(TIdentifier identifier, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return DoGet(identifier, scope);
    }

    public Task<TDomainEntity?> Get(TIdentifier identifier, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return DoGet(identifier, scope, cancellationToken);
    }

    public TDomainEntity? Get(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return DoGet(parameters, scope);
    }

    public Task<TDomainEntity?> Get(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return DoGet(parameters, scope, cancellationToken);
    }

    public TDomainEntity? Get(Object parameters, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return Get(parameters.ToFilterCollection(), scope);
    }

    public Task<TDomainEntity?> Get(Object parameters, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return Get(parameters.ToFilterCollection(), scope, cancellationToken);
    }

    public IReadOnlyCollection<TDomainEntity> GetList(TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return DoGetList(null, scope);
    }

    public Task<IReadOnlyCollection<TDomainEntity>> GetList(TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return DoGetList(null, scope, cancellationToken);
    }

    public IReadOnlyCollection<TDomainEntity> GetList(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return DoGetList(parameters, scope);
    }

    public Task<IReadOnlyCollection<TDomainEntity>> GetList(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return DoGetList(parameters, scope, cancellationToken);
    }

    public IReadOnlyCollection<TDomainEntity> GetList(Object parameters, TScope? scope)
    {
        return GetList(parameters.ToFilterCollection(), scope);
    }

    public Task<IReadOnlyCollection<TDomainEntity>> GetList(Object parameters, TScope? scope, CancellationToken cancellationToken)
    {
        return GetList(parameters.ToFilterCollection(), scope, cancellationToken);
    }

    protected abstract TDomainEntity? DoGet(TIdentifier identifier, TScope? scope);

    protected abstract Task<TDomainEntity?> DoGet(TIdentifier identifier, TScope? scope, CancellationToken cancellationToken);

    protected abstract TDomainEntity? DoGet(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope);

    protected abstract Task<TDomainEntity?> DoGet(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken);

    protected abstract IReadOnlyCollection<TDomainEntity> DoGetList(IReadOnlyCollection<FilterCriteria>? parameters, TScope? scope);

    protected abstract Task<IReadOnlyCollection<TDomainEntity>> DoGetList(IReadOnlyCollection<FilterCriteria>? parameters, TScope? scope, CancellationToken cancellationToken);
}
