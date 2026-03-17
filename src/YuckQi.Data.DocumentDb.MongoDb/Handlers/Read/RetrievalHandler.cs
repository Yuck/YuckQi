using MongoDB.Driver;
using YuckQi.Data.DocumentDb.MongoDb.Extensions;
using YuckQi.Data.DocumentDb.MongoDb.Filtering.Extensions;
using YuckQi.Data.Filtering;
using YuckQi.Data.Handlers.Read.Abstract;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.DocumentDb.MongoDb.Handlers.Read;

public class RetrievalHandler<TDomainEntity, TIdentifier, TScope> : RetrievalHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IClientSessionHandle?;

public class RetrievalHandler<TDomainEntity, TIdentifier, TScope, TDocument>(IMapper? mapper = null) : RetrievalHandlerBase<TDomainEntity, TIdentifier, TScope?, TDocument>(mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IClientSessionHandle?
{
    private static readonly Type DocumentType = typeof(TDocument);

    protected override TDomainEntity? DoGet(TIdentifier identifier, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var field = DocumentType.GetIdentifierFieldDefinition<TDocument, TIdentifier>();
        var filter = Builders<TDocument>.Filter.Eq(field, identifier);
        var reader = collection.FindSync(filter);
        var document = GetDocument(reader);
        var entity = MapToEntity(document);

        return entity;
    }

    protected override async Task<TDomainEntity?> DoGet(TIdentifier identifier, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var field = DocumentType.GetIdentifierFieldDefinition<TDocument, TIdentifier>();
        var filter = Builders<TDocument>.Filter.Eq(field, identifier);
        var reader = await collection.FindAsync(filter, cancellationToken: cancellationToken);
        var document = GetDocument(reader);
        var entity = MapToEntity(document);

        return entity;
    }

    protected override TDomainEntity? DoGet(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var filter = parameters.ToFilterDefinition<TDocument>();
        var reader = collection.FindSync(filter);
        var document = GetDocument(reader);
        var entity = MapToEntity(document);

        return entity;
    }

    protected override async Task<TDomainEntity?> DoGet(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var filter = parameters.ToFilterDefinition<TDocument>();
        var reader = await collection.FindAsync(filter, cancellationToken: cancellationToken);
        var document = GetDocument(reader);
        var entity = MapToEntity(document);

        return entity;
    }

    protected override IReadOnlyCollection<TDomainEntity> DoGetList(IReadOnlyCollection<FilterCriteria>? parameters, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var filter = parameters.ToFilterDefinition<TDocument>();
        var reader = collection.FindSync(filter);
        var documents = GetDocuments(reader);
        var entities = MapToEntityCollection(documents);

        return entities;
    }

    protected override async Task<IReadOnlyCollection<TDomainEntity>> DoGetList(IReadOnlyCollection<FilterCriteria>? parameters, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var filter = parameters.ToFilterDefinition<TDocument>();
        var reader = await collection.FindAsync(filter, cancellationToken: cancellationToken);
        var documents = GetDocuments(reader);
        var entities = MapToEntityCollection(documents);

        return entities;
    }

    private static TDocument? GetDocument(IAsyncCursor<TDocument> reader)
    {
        return reader.MoveNext() ? reader.Current.SingleOrDefault() : default;
    }

    private static IEnumerable<TDocument> GetDocuments(IAsyncCursor<TDocument> reader)
    {
        var documents = new List<TDocument>();

        while (reader.MoveNext())
            documents.AddRange(reader.Current);

        return documents;
    }
}
