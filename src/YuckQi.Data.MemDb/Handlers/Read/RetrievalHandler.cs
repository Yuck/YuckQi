using System.Collections.Concurrent;
using YuckQi.Data.Extensions;
using YuckQi.Data.Filtering;
using YuckQi.Data.Filtering.Extensions;
using YuckQi.Data.Handlers.Read.Abstract.Interfaces;
using YuckQi.Data.MemDb.Filtering.Extensions;
using YuckQi.Domain.Entities.Abstract.Interfaces;

namespace YuckQi.Data.MemDb.Handlers.Read;

public class RetrievalHandler<TDomainEntity, TIdentifier, TScope>(ConcurrentDictionary<TIdentifier, TDomainEntity> entities) : IRetrievalHandler<TDomainEntity, TIdentifier, TScope?> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier>
{
    public TDomainEntity? Get(TIdentifier identifier, TScope? scope)
    {
        return entities.TryGetValue(identifier, out var entity) ? entity : default;
    }

    public Task<TDomainEntity?> Get(TIdentifier identifier, TScope? scope, CancellationToken cancellationToken)
    {
        return Task.FromResult(Get(identifier, scope));
    }

    public TDomainEntity? Get(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope)
    {
        var entities = GetEntities(parameters);
        var entity = entities.SingleOrDefault();

        return entity;
    }

    public Task<TDomainEntity?> Get(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken)
    {
        return Task.FromResult(Get(parameters, scope));
    }

    public TDomainEntity? Get(Object parameters, TScope? scope)
    {
        return Get(parameters.ToFilterCollection(), scope);
    }

    public Task<TDomainEntity?> Get(Object parameters, TScope? scope, CancellationToken cancellationToken)
    {
        return Task.FromResult(Get(parameters, scope));
    }

    public IReadOnlyCollection<TDomainEntity> GetList(TScope? scope)
    {
        return [.. entities.Values.Select(t => t)];
    }

    public Task<IReadOnlyCollection<TDomainEntity>> GetList(TScope? scope, CancellationToken cancellationToken)
    {
        return Task.FromResult(GetList(scope));
    }

    public IReadOnlyCollection<TDomainEntity> GetList(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope)
    {
        return [.. GetEntities(parameters)];
    }

    public Task<IReadOnlyCollection<TDomainEntity>> GetList(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken)
    {
        return Task.FromResult(GetList(parameters, scope));
    }

    public IReadOnlyCollection<TDomainEntity> GetList(Object parameters, TScope? scope)
    {
        return GetList(parameters.ToFilterCollection(), scope);
    }

    public Task<IReadOnlyCollection<TDomainEntity>> GetList(Object parameters, TScope? scope, CancellationToken cancellationToken)
    {
        return Task.FromResult(GetList(parameters, scope));
    }

    private IEnumerable<TDomainEntity> GetEntities(IReadOnlyCollection<FilterCriteria> parameters)
    {
        return entities.Values.Where(entity => parameters.Select(t => t.ToExpression(entity)).All(t => t()));
    }
}
