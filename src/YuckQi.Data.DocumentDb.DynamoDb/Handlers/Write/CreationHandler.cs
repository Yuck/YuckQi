using Amazon.DynamoDBv2.DataModel;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.DocumentDb.DynamoDb.Handlers.Write;

public class CreationHandler<TDomainEntity, TIdentifier, TScope>(CreationOptions<TIdentifier>? options = null) : CreationHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity>(options, null) where TDomainEntity : IDomainEntity<TIdentifier>, ICreationMoment where TIdentifier : IEquatable<TIdentifier> where TScope : IDynamoDBContext?;

public class CreationHandler<TDomainEntity, TIdentifier, TScope, TDocument>(CreationOptions<TIdentifier>? options = null, IMapper? mapper = null) : CreationHandlerBase<TDomainEntity, TIdentifier, TScope?, TDocument>(options, mapper) where TDomainEntity : IDomainEntity<TIdentifier>, ICreationMoment where TIdentifier : IEquatable<TIdentifier> where TScope : IDynamoDBContext?
{
    public override IEnumerable<TDomainEntity> Create(IEnumerable<TDomainEntity> entities, TScope? scope)
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

    public override async Task<IEnumerable<TDomainEntity>> Create(IEnumerable<TDomainEntity> entities, TScope? scope, CancellationToken cancellationToken)
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

    protected override TIdentifier? DoCreate(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var task = Task.Run(async () => await DoCreate(entity, scope, CancellationToken.None));
        var result = task.GetAwaiter().GetResult();

        return result;
    }

    protected override async Task<TIdentifier?> DoCreate(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var document = MapToData(entity) ?? throw new InvalidOperationException($"Failed to map {typeof(TDomainEntity).Name} entity to {typeof(TDocument).Name} document.");
        var table = scope.GetTargetTable<TDocument>();

        await table.PutItemAsync(scope.ToDocument(document), cancellationToken);

        return entity.Identifier;
    }
}
