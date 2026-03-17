using Amazon.DynamoDBv2.DataModel;
using YuckQi.Data.DocumentDb.DynamoDb.Filtering.Extensions;
using YuckQi.Data.Filtering;
using YuckQi.Data.Handlers.Read.Abstract;
using YuckQi.Data.Sorting;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Domain.ValueObjects.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.DocumentDb.DynamoDb.Handlers.Read;

public class SearchHandler<TDomainEntity, TIdentifier, TScope> : SearchHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IDynamoDBContext?;

public class SearchHandler<TDomainEntity, TIdentifier, TScope, TDocument>(IMapper? mapper = null) : SearchHandlerBase<TDomainEntity, TIdentifier, TScope?, TDocument>(mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IDynamoDBContext?
{
    protected override Int32 DoCount(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var task = Task.Run(async () => await DoCount(parameters, scope, CancellationToken.None));
        var result = task.GetAwaiter().GetResult();

        return result;
    }

    protected override Task<Int32> DoCount(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var table = scope.GetTargetTable<TDocument>();
        var filter = parameters.ToScanFilter();
        var search = table.Scan(filter);
        var count = search.Count;

        return Task.FromResult(count);
    }

    protected override IReadOnlyCollection<TDomainEntity> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var task = Task.Run(async () => await DoSearch(parameters, page, sort, scope, CancellationToken.None));
        var result = task.GetAwaiter().GetResult();

        return result;
    }

    protected override Task<IReadOnlyCollection<TDomainEntity>> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
