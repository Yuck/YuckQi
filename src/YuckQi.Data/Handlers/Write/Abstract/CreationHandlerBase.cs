using YuckQi.Data.Exceptions;
using YuckQi.Data.Handlers.Write.Abstract.Interfaces;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.Handlers.Write.Abstract;

public abstract class CreationHandlerBase<TDomainEntity, TIdentifier, TScope>(CreationOptions<TIdentifier>? options = null, IMapper? mapper = null) : CreationHandlerBase<TDomainEntity, TIdentifier, TScope, TDomainEntity>(options, mapper) where TDomainEntity : IDomainEntity<TIdentifier>, ICreationMoment where TIdentifier : IEquatable<TIdentifier>;

public abstract class CreationHandlerBase<TDomainEntity, TIdentifier, TScope, TData>(CreationOptions<TIdentifier>? options = null, IMapper? mapper = null) : HandlerBase<TDomainEntity, TData>(mapper), ICreationHandler<TDomainEntity, TIdentifier, TScope?> where TDomainEntity : IDomainEntity<TIdentifier>, ICreationMoment where TIdentifier : IEquatable<TIdentifier>
{
    private readonly CreationOptions<TIdentifier> _options = options ?? new CreationOptions<TIdentifier>();

    public TDomainEntity Create(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        entity = PreProcess(entity);
        entity.Identifier = DoCreate(entity, scope) ?? throw new CreationException<TDomainEntity>();

        return entity;
    }

    public virtual IEnumerable<TDomainEntity> Create(IEnumerable<TDomainEntity> entities, TScope? scope)
    {
        return entities.Select(entity => Create(entity, scope));
    }

    public async Task<TDomainEntity> Create(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        entity = PreProcess(entity);
        entity.Identifier = await DoCreate(entity, scope, cancellationToken) ?? throw new CreationException<TDomainEntity>();

        return entity;
    }

    public virtual async Task<IEnumerable<TDomainEntity>> Create(IEnumerable<TDomainEntity> entities, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var tasks = entities.Select(entity => Create(entity, scope, cancellationToken));
        var results = await Task.WhenAll(tasks);

        return results;
    }

    protected abstract TIdentifier? DoCreate(TDomainEntity entity, TScope? scope);

    protected abstract Task<TIdentifier?> DoCreate(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken);

    protected TDomainEntity PreProcess(TDomainEntity entity)
    {
        if (_options.IdentifierFactory != null)
            entity.Identifier = _options.IdentifierFactory();
        if (_options.CreationMomentAssignment == PropertyHandling.Auto)
            entity.CreationMoment = DateTime.UtcNow;
        if (_options.RevisionMomentAssignment == PropertyHandling.Auto && entity is IRevisionMoment revised)
            revised.RevisionMoment = entity.CreationMoment;

        return entity;
    }
}
