using Amazon.DynamoDBv2.DataModel;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.DocumentDb.DynamoDb.Handlers.Write;

public class PhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope> : PhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IDynamoDBContext?;

public class PhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope, TDocument>(IMapper? mapper = null) : PhysicalDeletionHandlerBase<TDomainEntity, TIdentifier, TScope?, TDocument>(mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IDynamoDBContext?
{
    protected override Boolean DoDelete(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var task = Task.Run(async () => await DoDelete(entity, scope, CancellationToken.None));
        var result = task.GetAwaiter().GetResult();

        return result;
    }

    protected override async Task<Boolean> DoDelete(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var document = MapToData(entity) ?? throw new InvalidOperationException("Failed to map entity to document.");
        var table = scope.GetTargetTable<TDocument>();

        await table.DeleteItemAsync(scope.ToDocument(document), cancellationToken);

        return true;
    }
}
