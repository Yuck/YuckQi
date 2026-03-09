using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using YuckQi.Data.DocumentDb.DynamoDb.Extensions;
using YuckQi.Data.Filtering;
using YuckQi.Data.Handlers.Read.Abstract;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.DocumentDb.DynamoDb.Handlers.Read;

public class RetrievalHandler<TDomainEntity, TIdentifier, TScope>(Func<TIdentifier, Primitive> hashKeyValueFactory, Func<TIdentifier, Primitive>? rangeKeyValueFactory = null) : RetrievalHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity>(hashKeyValueFactory, rangeKeyValueFactory, null) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IDynamoDBContext?;

public class RetrievalHandler<TDomainEntity, TIdentifier, TScope, TDocument>(Func<TIdentifier, Primitive> hashKeyValueFactory, Func<TIdentifier, Primitive>? rangeKeyValueFactory = null, IMapper? mapper = null) : RetrievalHandlerBase<TDomainEntity, TIdentifier, TScope?, TDocument>(mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IDynamoDBContext?
{
    protected override TDomainEntity? DoGet(TIdentifier identifier, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var task = Task.Run(async () => await DoGet(identifier, scope, CancellationToken.None));
        var result = task.GetAwaiter().GetResult();

        return result;
    }

    protected override async Task<TDomainEntity?> DoGet(TIdentifier identifier, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var table = scope.GetTargetTable<TDocument>();
        var hashKey = hashKeyValueFactory(identifier);
        var rangeKey = rangeKeyValueFactory?.Invoke(identifier);
        var result = rangeKey != null
                         ? await table.GetItemAsync(hashKey, rangeKey, cancellationToken)
                         : await table.GetItemAsync(hashKey, cancellationToken);
        var document = scope.FromDocument<TDocument>(result);
        var entity = MapToEntity(document);

        return entity;
    }

    protected override TDomainEntity? DoGet(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var task = Task.Run(async () => await DoGet(parameters, scope, CancellationToken.None));
        var result = task.GetAwaiter().GetResult();

        return result;
    }

    protected override async Task<TDomainEntity?> DoGet(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var table = scope.GetTargetTable<TDocument>();
        var filter = parameters.ToQueryFilter();
        var search = table.Query(filter) as Search;
        var documents = await GetDocuments(scope, search, cancellationToken);
        var document = documents.SingleOrDefault();
        var entity = MapToEntity(document);

        return entity;
    }

    protected override IReadOnlyCollection<TDomainEntity> DoGetList(IReadOnlyCollection<FilterCriteria>? parameters, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var task = Task.Run(async () => await DoGetList(parameters, scope, CancellationToken.None));
        var result = task.GetAwaiter().GetResult();

        return result;
    }

    protected override async Task<IReadOnlyCollection<TDomainEntity>> DoGetList(IReadOnlyCollection<FilterCriteria>? parameters, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var table = scope.GetTargetTable<TDocument>();
        var filter = parameters?.ToQueryFilter();
        var search = table.Query(filter) as Search;
        var documents = await GetDocuments(scope, search, cancellationToken);
        var entities = MapToEntityCollection(documents);

        return entities;
    }

    private static async Task<IEnumerable<TDocument>> GetDocuments(TScope? scope, Search? search, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(search);

        var documents = new List<Document>();

        while (! search.IsDone)
            documents.AddRange(await search.GetNextSetAsync(cancellationToken));
        documents.AddRange(await search.GetRemainingAsync(cancellationToken));

        return scope.FromDocuments<TDocument>(documents);
    }
}
