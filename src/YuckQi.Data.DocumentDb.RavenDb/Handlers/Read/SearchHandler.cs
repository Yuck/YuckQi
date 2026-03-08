using Raven.Client.Documents.Session;
using YuckQi.Data.DocumentDb.RavenDb.Extensions;
using YuckQi.Data.Filtering;
using YuckQi.Data.Handlers.Read.Abstract;
using YuckQi.Data.Sorting;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Domain.ValueObjects.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.DocumentDb.RavenDb.Handlers.Read;

public class SearchHandler<TDomainEntity, TIdentifier, TScope> : SearchHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IAsyncDocumentSession?;

public class SearchHandler<TDomainEntity, TIdentifier, TScope, TDocument>(IMapper? mapper = null) : SearchHandlerBase<TDomainEntity, TIdentifier, TScope?, TDocument>(mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IAsyncDocumentSession?
{
    protected override Int32 DoCount(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var task = DoCount(parameters, scope, CancellationToken.None);
        var result = task.GetAwaiter().GetResult();

        return result;
    }

    protected override async Task<Int32> DoCount(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var query = scope.Advanced.AsyncDocumentQuery<TDocument>().ApplyFilter(parameters);
        var total = await query.CountAsync(cancellationToken);

        return total;
    }

    protected override IReadOnlyCollection<TDomainEntity> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var task = DoSearch(parameters, page, sort, scope, CancellationToken.None);
        var result = task.GetAwaiter().GetResult();

        return result;
    }

    protected override async Task<IReadOnlyCollection<TDomainEntity>> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var query = sort.Aggregate(scope.Advanced.AsyncDocumentQuery<TDocument>().ApplyFilter(parameters), (t, u) => u.Order == SortOrder.Ascending ? t.OrderBy(u.Expression) : t.OrderByDescending(u.Expression));
        var documents = await query.Skip((page.PageNumber - 1) * page.PageSize)
                                   .Take(page.PageSize)
                                   .ToListAsync(cancellationToken);
        var entities = MapToEntityCollection(documents);

        return entities;
    }
}
