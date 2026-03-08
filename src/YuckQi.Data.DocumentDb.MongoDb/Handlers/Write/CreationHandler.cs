using MongoDB.Driver;
using YuckQi.Data.DocumentDb.MongoDb.Extensions;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.DocumentDb.MongoDb.Handlers.Write;

public class CreationHandler<TDomainEntity, TIdentifier, TScope>(CreationOptions<TIdentifier>? options = null) : CreationHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity>(options) where TDomainEntity : IDomainEntity<TIdentifier>, ICreationMoment where TIdentifier : IEquatable<TIdentifier> where TScope : IClientSessionHandle?;

public class CreationHandler<TDomainEntity, TIdentifier, TScope, TDocument>(CreationOptions<TIdentifier>? options = null, IMapper? mapper = null) : CreationHandlerBase<TDomainEntity, TIdentifier, TScope?, TDocument>(options, mapper) where TDomainEntity : IDomainEntity<TIdentifier>, ICreationMoment where TIdentifier : IEquatable<TIdentifier> where TScope : IClientSessionHandle?
{
    private static readonly Type DocumentType = typeof(TDocument);

    public override IEnumerable<TDomainEntity> Create(IEnumerable<TDomainEntity> entities, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var list = entities.Select(PreProcess).ToList();
        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var documents = MapToDataCollection(list) ?? throw new InvalidOperationException($"Failed to map {typeof(TDomainEntity).Name} entities to {typeof(TDocument).Name} documents.");

        collection.InsertMany(scope, documents);

        return list;
    }

    public override async Task<IEnumerable<TDomainEntity>> Create(IEnumerable<TDomainEntity> entities, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var list = entities.Select(PreProcess).ToList();
        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var documents = MapToDataCollection(list) ?? throw new InvalidOperationException($"Failed to map {typeof(TDomainEntity).Name} entities to {typeof(TDocument).Name} documents.");

        await collection.InsertManyAsync(scope, documents, cancellationToken: cancellationToken);

        return list;
    }

    protected override TIdentifier? DoCreate(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var document = MapToData(entity) ?? throw new InvalidOperationException($"Failed to map {typeof(TDomainEntity).Name} entity to {typeof(TDocument).Name} document.");

        collection.InsertOne(scope, document);

        return document.GetIdentifier<TDocument, TIdentifier>();
    }

    protected override async Task<TIdentifier?> DoCreate(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var document = MapToData(entity) ?? throw new InvalidOperationException($"Failed to map {typeof(TDomainEntity).Name} entity to {typeof(TDocument).Name} document.");

        await collection.InsertOneAsync(scope, document, cancellationToken: cancellationToken);

        return document.GetIdentifier<TDocument, TIdentifier>();
    }
}
