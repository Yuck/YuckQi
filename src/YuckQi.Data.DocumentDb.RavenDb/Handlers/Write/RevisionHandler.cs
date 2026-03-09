using Raven.Client.Documents.Session;
using YuckQi.Data.DocumentDb.RavenDb.Extensions;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.DocumentDb.RavenDb.Handlers.Write;

public class RevisionHandler<TDomainEntity, TIdentifier, TScope>(RevisionOptions? options = null) : RevisionHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity>(options) where TDomainEntity : IDomainEntity<TIdentifier>, IRevisionMoment where TIdentifier : IEquatable<TIdentifier> where TScope : IAsyncDocumentSession?;

public class RevisionHandler<TDomainEntity, TIdentifier, TScope, TDocument>(RevisionOptions? options = null, IMapper? mapper = null) : RevisionHandlerBase<TDomainEntity, TIdentifier, TScope?, TDocument>(options, mapper) where TDomainEntity : IDomainEntity<TIdentifier>, IRevisionMoment where TIdentifier : IEquatable<TIdentifier> where TScope : IAsyncDocumentSession?
{
    public override IEnumerable<TDomainEntity> Revise(IEnumerable<TDomainEntity> entities, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var list = entities.Select(PreProcess).ToList();
        var documents = MapToDataCollection(list) ?? throw new InvalidOperationException($"Failed to map {typeof(TDomainEntity).Name} entities to {typeof(TDocument).Name} documents.");
        var documentList = documents.ToList();

        for (var i = 0; i < list.Count; i++)
        {
            documentList[i].SetDocumentId(list[i].Identifier);

            scope.StoreAsync(documentList[i], CancellationToken.None).GetAwaiter().GetResult();
        }

        return list;
    }

    public override async Task<IEnumerable<TDomainEntity>> Revise(IEnumerable<TDomainEntity> entities, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var list = entities.Select(PreProcess).ToList();
        var documents = MapToDataCollection(list) ?? throw new InvalidOperationException($"Failed to map {typeof(TDomainEntity).Name} entities to {typeof(TDocument).Name} documents.");
        var documentList = documents.ToList();

        for (var i = 0; i < list.Count; i++)
        {
            documentList[i].SetDocumentId(list[i].Identifier);

            await scope.StoreAsync(documentList[i], cancellationToken);
        }

        return list;
    }

    protected override Boolean DoRevise(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var task = DoRevise(entity, scope, CancellationToken.None);
        var result = task.GetAwaiter().GetResult();

        return result;
    }

    protected override async Task<Boolean> DoRevise(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var document = MapToData(entity) ?? throw new InvalidOperationException($"Failed to map {typeof(TDomainEntity).Name} entity to {typeof(TDocument).Name} document.");

        document.SetDocumentId(entity.Identifier);

        await scope.StoreAsync(document, cancellationToken);

        return true;
    }
}
