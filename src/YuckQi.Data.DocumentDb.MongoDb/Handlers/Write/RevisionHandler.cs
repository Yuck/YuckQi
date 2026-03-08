using MongoDB.Driver;
using YuckQi.Data.DocumentDb.MongoDb.Extensions;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.DocumentDb.MongoDb.Handlers.Write;

public class RevisionHandler<TDomainEntity, TIdentifier, TScope>(RevisionOptions? options = null) : RevisionHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity>(options) where TDomainEntity : IDomainEntity<TIdentifier>, IRevisionMoment where TIdentifier : IEquatable<TIdentifier> where TScope : IClientSessionHandle?;

public class RevisionHandler<TDomainEntity, TIdentifier, TScope, TDocument>(RevisionOptions? options = null, IMapper? mapper = null) : RevisionHandlerBase<TDomainEntity, TIdentifier, TScope?, TDocument>(options, mapper) where TDomainEntity : IDomainEntity<TIdentifier>, IRevisionMoment where TIdentifier : IEquatable<TIdentifier> where TScope : IClientSessionHandle?
{
    private static readonly Type DocumentType = typeof(TDocument);

    public override IEnumerable<TDomainEntity> Revise(IEnumerable<TDomainEntity> entities, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var list = entities.Select(PreProcess).ToList();
        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var field = DocumentType.GetIdentifierFieldDefinition<TDocument, TIdentifier>();
        var documents = MapToDataCollection(list) ?? throw new InvalidOperationException($"Failed to map {typeof(TDomainEntity).Name} entities to {typeof(TDocument).Name} documents.");

        foreach (var document in documents)
        {
            var identifier = document != null ? document.GetIdentifier<TDocument, TIdentifier>() : default;
            var filter = Builders<TDocument>.Filter.Eq(field, identifier);

            collection.ReplaceOne(scope, filter, document);
        }

        return list;
    }

    public override async Task<IEnumerable<TDomainEntity>> Revise(IEnumerable<TDomainEntity> entities, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var list = entities.Select(PreProcess).ToList();
        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var field = DocumentType.GetIdentifierFieldDefinition<TDocument, TIdentifier>();
        var documents = MapToDataCollection(list) ?? throw new InvalidOperationException($"Failed to map {typeof(TDomainEntity).Name} entities to {typeof(TDocument).Name} documents.");

        foreach (var document in documents)
        {
            var identifier = document != null ? document.GetIdentifier<TDocument, TIdentifier>() : default;
            var filter = Builders<TDocument>.Filter.Eq(field, identifier);

            await collection.ReplaceOneAsync(scope, filter, document, cancellationToken: cancellationToken);
        }

        return list;
    }

    protected override Boolean DoRevise(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var field = DocumentType.GetIdentifierFieldDefinition<TDocument, TIdentifier>();
        var document = MapToData(entity) ?? throw new InvalidOperationException($"Failed to map {typeof(TDomainEntity).Name} entity to {typeof(TDocument).Name} document.");
        var identifier = document.GetIdentifier<TDocument, TIdentifier>();
        var filter = Builders<TDocument>.Filter.Eq(field, identifier);

        var result = collection.ReplaceOne(scope, filter, document);

        return result.ModifiedCount > 0 || result.MatchedCount > 0;
    }

    protected override async Task<Boolean> DoRevise(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var field = DocumentType.GetIdentifierFieldDefinition<TDocument, TIdentifier>();
        var document = MapToData(entity) ?? throw new InvalidOperationException($"Failed to map {typeof(TDomainEntity).Name} entity to {typeof(TDocument).Name} document.");
        var identifier = document.GetIdentifier<TDocument, TIdentifier>();
        var filter = Builders<TDocument>.Filter.Eq(field, identifier);

        var result = await collection.ReplaceOneAsync(scope, filter, document, cancellationToken: cancellationToken);

        return result.ModifiedCount > 0 || result.MatchedCount > 0;
    }
}
