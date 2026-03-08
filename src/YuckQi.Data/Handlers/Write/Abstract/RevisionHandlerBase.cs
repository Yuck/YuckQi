using YuckQi.Data.Exceptions;
using YuckQi.Data.Handlers.Write.Abstract.Interfaces;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.Handlers.Write.Abstract;

public abstract class RevisionHandlerBase<TDomainEntity, TIdentifier, TScope>(RevisionOptions? options = null, IMapper? mapper = null) : RevisionHandlerBase<TDomainEntity, TIdentifier, TScope, TDomainEntity>(options, mapper) where TDomainEntity : IDomainEntity<TIdentifier>, IRevisionMoment where TIdentifier : IEquatable<TIdentifier>;

public abstract class RevisionHandlerBase<TDomainEntity, TIdentifier, TScope, TData>(RevisionOptions? options = null, IMapper? mapper = null) : HandlerBase<TDomainEntity, TData>(mapper), IRevisionHandler<TDomainEntity, TIdentifier, TScope?> where TDomainEntity : IDomainEntity<TIdentifier>, IRevisionMoment where TIdentifier : IEquatable<TIdentifier>
{
    private readonly RevisionOptions _options = options ?? new RevisionOptions();

    public TDomainEntity Revise(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return DoRevise(PreProcess(entity), scope) ? entity : throw new RevisionException<TDomainEntity, TIdentifier>(entity.Identifier);
    }

    public virtual IEnumerable<TDomainEntity> Revise(IEnumerable<TDomainEntity> entities, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return entities.Select(entity => Revise(entity, scope));
    }

    public async Task<TDomainEntity> Revise(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return await DoRevise(PreProcess(entity), scope, cancellationToken) ? entity : throw new RevisionException<TDomainEntity, TIdentifier>(entity.Identifier);
    }

    public virtual async Task<IEnumerable<TDomainEntity>> Revise(IEnumerable<TDomainEntity> entities, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var tasks = entities.Select(entity => Revise(entity, scope, cancellationToken));
        var results = await Task.WhenAll(tasks);

        return results;
    }

    protected abstract Boolean DoRevise(TDomainEntity entity, TScope? scope);

    protected abstract Task<Boolean> DoRevise(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken);

    protected TDomainEntity PreProcess(TDomainEntity entity)
    {
        if (_options.RevisionMomentAssignment == PropertyHandling.Auto)
            entity.RevisionMoment = DateTime.UtcNow;

        return entity;
    }
}
