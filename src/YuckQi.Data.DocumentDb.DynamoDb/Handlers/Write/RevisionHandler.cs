using Amazon.DynamoDBv2.DataModel;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.DocumentDb.DynamoDb.Handlers.Write;

public class RevisionHandler<TDomainEntity, TIdentifier, TScope>(RevisionOptions? options = null) : RevisionHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity>(options, null) where TDomainEntity : IDomainEntity<TIdentifier>, IRevisionMoment where TIdentifier : IEquatable<TIdentifier> where TScope : IDynamoDBContext?;

public class RevisionHandler<TDomainEntity, TIdentifier, TScope, TDocument>(RevisionOptions? options = null, IMapper? mapper = null) : RevisionHandlerBase<TDomainEntity, TIdentifier, TScope?, TDocument>(options, mapper) where TDomainEntity : IDomainEntity<TIdentifier>, IRevisionMoment where TIdentifier : IEquatable<TIdentifier> where TScope : IDynamoDBContext?
{
    public override IEnumerable<TDomainEntity> Revise(IEnumerable<TDomainEntity> entities, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var table = scope.GetTargetTable<TDocument>();
        var batch = table.CreateBatchWrite();
        var list = entities.ToList();
        var documents = list.Select(MapToData);

        foreach (var document in documents)
            batch.AddDocumentToPut(scope.ToDocument(document));

        Task.Run(async () => await batch.ExecuteAsync()).GetAwaiter().GetResult();

        return list;
    }

    public override async Task<IEnumerable<TDomainEntity>> Revise(IEnumerable<TDomainEntity> entities, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var table = scope.GetTargetTable<TDocument>();
        var batch = table.CreateBatchWrite();
        var list = entities.ToList();
        var documents = list.Select(MapToData);

        foreach (var document in documents)
            batch.AddDocumentToPut(scope.ToDocument(document));

        await batch.ExecuteAsync(cancellationToken);

        return list;
    }

    protected override Boolean DoRevise(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var task = Task.Run(async () => await DoRevise(entity, scope, CancellationToken.None));
        var result = task.GetAwaiter().GetResult();

        return result;
    }

    protected override async Task<Boolean> DoRevise(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var document = MapToData(entity) ?? throw new InvalidOperationException($"Failed to map {typeof(TDomainEntity).Name} entity to {typeof(TDocument).Name} document.");
        var table = scope.GetTargetTable<TDocument>();

        await table.PutItemAsync(scope.ToDocument(document), cancellationToken);

        return true;
    }
}
