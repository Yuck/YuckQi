using Raven.Client.Documents.Session;
using YuckQi.Data.DocumentDb.RavenDb.Extensions;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.DocumentDb.RavenDb.Handlers.Write;

public class CreationHandler<TDomainEntity, TIdentifier, TScope>(CreationOptions<TIdentifier>? options = null) : CreationHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity>(options) where TDomainEntity : IDomainEntity<TIdentifier>, ICreationMoment where TIdentifier : IEquatable<TIdentifier> where TScope : IAsyncDocumentSession?;

public class CreationHandler<TDomainEntity, TIdentifier, TScope, TDocument>(CreationOptions<TIdentifier>? options = null, IMapper? mapper = null) : CreationHandlerBase<TDomainEntity, TIdentifier, TScope?, TDocument>(options, mapper) where TDomainEntity : IDomainEntity<TIdentifier>, ICreationMoment where TIdentifier : IEquatable<TIdentifier> where TScope : IAsyncDocumentSession?
{
    public override IEnumerable<TDomainEntity> Create(IEnumerable<TDomainEntity> entities, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var list = entities.Select(PreProcess).ToList();
        var documents = MapToDataCollection(list) ?? throw new InvalidOperationException($"Failed to map {typeof(TDomainEntity).Name} entities to {typeof(TDocument).Name} documents.");

        foreach (var document in documents)
            scope.StoreAsync(document, CancellationToken.None).GetAwaiter().GetResult();

        return list;
    }

    public override async Task<IEnumerable<TDomainEntity>> Create(IEnumerable<TDomainEntity> entities, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var list = entities.Select(PreProcess).ToList();
        var documents = MapToDataCollection(list) ?? throw new InvalidOperationException($"Failed to map {typeof(TDomainEntity).Name} entities to {typeof(TDocument).Name} documents.");

        foreach (var document in documents)
            await scope.StoreAsync(document, cancellationToken);

        return list;
    }

    protected override TIdentifier? DoCreate(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var task = DoCreate(entity, scope, CancellationToken.None);
        var result = task.GetAwaiter().GetResult();

        return result;
    }

    protected override async Task<TIdentifier?> DoCreate(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var document = MapToData(entity) ?? throw new InvalidOperationException($"Failed to map {typeof(TDomainEntity).Name} entity to {typeof(TDocument).Name} document.");

        if (entity.Identifier != null)
            document.SetDocumentId(entity.Identifier);

        await scope.StoreAsync(document, cancellationToken);

        return document.GetIdentifier<TDocument, TIdentifier>();
    }
}
