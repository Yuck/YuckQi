using MongoDB.Driver;
using YuckQi.Data.DocumentDb.MongoDb.Extensions;
using YuckQi.Data.Filtering;
using YuckQi.Data.Handlers.Read.Abstract;
using YuckQi.Data.Sorting;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Domain.ValueObjects.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.DocumentDb.MongoDb.Handlers.Read;

public class SearchHandler<TDomainEntity, TIdentifier, TScope> : SearchHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IClientSessionHandle?;

public class SearchHandler<TDomainEntity, TIdentifier, TScope, TDocument>(IMapper? mapper = null) : SearchHandlerBase<TDomainEntity, TIdentifier, TScope?, TDocument>(mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IClientSessionHandle?
{
    private static readonly Type DocumentType = typeof(TDocument);

    protected override Int32 DoCount(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var filter = parameters.ToFilterDefinition<TDocument>();
        var total = collection.CountDocuments(filter);

        return (Int32) total;
    }

    protected override async Task<Int32> DoCount(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var filter = parameters.ToFilterDefinition<TDocument>();
        var total = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        return (Int32) total;
    }

    protected override IReadOnlyCollection<TDomainEntity> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var filter = parameters.ToFilterDefinition<TDocument>();
        var documents = collection.Find(filter)
                                  .Sort(GetSortDefinition(sort))
                                  .Skip((page.PageNumber - 1) * page.PageSize)
                                  .Limit(page.PageSize)
                                  .ToList();
        var entities = MapToEntityCollection(documents);

        return entities;
    }

    protected override async Task<IReadOnlyCollection<TDomainEntity>> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var database = scope.Client.GetDatabase(DocumentType.GetDatabaseName());
        var collection = database.GetCollection<TDocument>(DocumentType.GetCollectionName());
        var filter = parameters.ToFilterDefinition<TDocument>();
        var documents = await collection.Find(filter)
                                        .Sort(GetSortDefinition(sort))
                                        .Skip((page.PageNumber - 1) * page.PageSize)
                                        .Limit(page.PageSize)
                                        .ToListAsync(cancellationToken);
        var entities = MapToEntityCollection(documents);

        return entities;
    }

    private static SortDefinition<TDocument> GetSortDefinition(IEnumerable<SortCriteria> criteria)
    {
        var sorts = criteria.Select(t =>
        {
            var field = new StringFieldDefinition<TDocument>(t.Expression);
            var sort = t.Order == SortOrder.Ascending ? Builders<TDocument>.Sort.Ascending(field) : Builders<TDocument>.Sort.Descending(field);

            return sort;
        });
        var combined = Builders<TDocument>.Sort.Combine(sorts);

        return combined;
    }
}
