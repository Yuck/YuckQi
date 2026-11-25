using System.Collections.Concurrent;
using System.Linq.Expressions;
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

    public TDomainEntity? Get(Expression<Func<TDomainEntity, Boolean>> expression, TScope? scope)
    {
        var predicate = expression.Compile();

        return entities.Values.SingleOrDefault(predicate);
    }

    public Task<TDomainEntity?> Get(Expression<Func<TDomainEntity, Boolean>> expression, TScope? scope, CancellationToken cancellationToken)
    {
        return Task.FromResult(Get(expression, scope));
    }

    public TDomainEntity? Get(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope)
    {
        ThrowIfEmptyOrInvalid(parameters);

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
        if (parameters is TIdentifier identifier)
            return Get(identifier, scope);

        return Get(parameters.ToFilterCollection(), scope);
    }

    public Task<TDomainEntity?> Get(Object parameters, TScope? scope, CancellationToken cancellationToken)
    {
        return Task.FromResult(Get(parameters, scope));
    }

    public IReadOnlyCollection<TDomainEntity> GetList(TScope? scope)
    {
        return [.. entities.Values];
    }

    public Task<IReadOnlyCollection<TDomainEntity>> GetList(TScope? scope, CancellationToken cancellationToken)
    {
        return Task.FromResult(GetList(scope));
    }

    public IReadOnlyCollection<TDomainEntity> GetList(Expression<Func<TDomainEntity, Boolean>> expression, TScope? scope)
    {
        var predicate = expression.Compile();

        return [.. entities.Values.Where(predicate)];
    }

    public Task<IReadOnlyCollection<TDomainEntity>> GetList(Expression<Func<TDomainEntity, Boolean>> expression, TScope? scope, CancellationToken cancellationToken)
    {
        return Task.FromResult(GetList(expression, scope));
    }

    public IReadOnlyCollection<TDomainEntity> GetList(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope)
    {
        ThrowIfEmptyOrInvalid(parameters);

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

    private static void ThrowIfEmptyOrInvalid(IReadOnlyCollection<FilterCriteria> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (parameters.Count == 0)
            throw new ArgumentException("At least one filter is required. To get every entity, call GetList with only a scope.", nameof(parameters));

        var index = 0;

        foreach (var parameter in parameters)
        {
            if (parameter is null)
                throw new ArgumentException($"The filter at index {index} is null.", nameof(parameters));

            if (parameter.Operation == FilterOperation.Unknown)
                throw new ArgumentException($"The filter on '{parameter.FieldName}' does not specify a {nameof(FilterOperation)}.", nameof(parameters));

            index++;
        }
    }

    private IEnumerable<TDomainEntity> GetEntities(IReadOnlyCollection<FilterCriteria> parameters)
    {
        return entities.Values.Where(entity => parameters.Select(t => t.ToExpression(entity)).All(t => t()));
    }
}
