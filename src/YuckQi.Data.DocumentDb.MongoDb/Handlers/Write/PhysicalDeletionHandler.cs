using MongoDB.Driver;
using YuckQi.Data.DocumentDb.MongoDb.Extensions;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.DocumentDb.MongoDb.Handlers.Write;

public class PhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope> : PhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IClientSessionHandle?;

public class PhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope, TDocument>(IMapper? mapper = null) : PhysicalDeletionHandlerBase<TDomainEntity, TIdentifier, TScope?, TDocument>(mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IClientSessionHandle?
{
    private static readonly Type DocumentType = typeof(TDocument);

    protected override Boolean DoDelete(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var document = GetDocument(entity);
        var field = DocumentType.GetIdentifierFieldDefinition<TDocument, TIdentifier>();
        var identifier = document != null ? document.GetIdentifier<TDocument, TIdentifier>() : default;
        var filter = Builders<TDocument>.Filter.Eq(field, identifier);
        var result = collection.DeleteOne(scope, filter);

        return result.DeletedCount > 0;
    }

    protected override async Task<Boolean> DoDelete(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var document = GetDocument(entity);
        var field = DocumentType.GetIdentifierFieldDefinition<TDocument, TIdentifier>();
        var identifier = document != null ? document.GetIdentifier<TDocument, TIdentifier>() : default;
        var filter = Builders<TDocument>.Filter.Eq(field, identifier);
        var result = await collection.DeleteOneAsync(scope, filter, cancellationToken: cancellationToken);

        return result.DeletedCount > 0;
    }

    private TDocument? GetDocument(TDomainEntity entity)
    {
        if (entity is TDocument document)
            return document;

        return Mapper != null ? Mapper.Map<TDocument>(entity) : default;
    }
}
