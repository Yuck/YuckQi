using Raven.Client.Documents.Session;
using YuckQi.Data.DocumentDb.RavenDb.Extensions;
using YuckQi.Data.Filtering;
using YuckQi.Data.Handlers.Read.Abstract;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.DocumentDb.RavenDb.Handlers.Read;

public class RetrievalHandler<TDomainEntity, TIdentifier, TScope> : RetrievalHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IAsyncDocumentSession?;

public class RetrievalHandler<TDomainEntity, TIdentifier, TScope, TDocument>(IMapper? mapper = null) : RetrievalHandlerBase<TDomainEntity, TIdentifier, TScope?, TDocument>(mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IAsyncDocumentSession?
{
    protected override TDomainEntity? DoGet(TIdentifier identifier, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var task = DoGet(identifier, scope, CancellationToken.None);
        var result = task.GetAwaiter().GetResult();

        return result;
    }

    protected override async Task<TDomainEntity?> DoGet(TIdentifier identifier, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var id = identifier.ToDocumentId();
        var document = await scope.LoadAsync<TDocument>(id, cancellationToken);
        var entity = MapToEntity(document);

        return entity;
    }

    protected override TDomainEntity? DoGet(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var task = DoGet(parameters, scope, CancellationToken.None);
        var result = task.GetAwaiter().GetResult();

        return result;
    }

    protected override async Task<TDomainEntity?> DoGet(IReadOnlyCollection<FilterCriteria> parameters, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var query = scope.Advanced.AsyncDocumentQuery<TDocument>().ApplyFilter(parameters);
        var document = await query.FirstOrDefaultAsync(cancellationToken);
        var entity = MapToEntity(document);

        return entity;
    }

    protected override IReadOnlyCollection<TDomainEntity> DoGetList(IReadOnlyCollection<FilterCriteria>? parameters, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var task = DoGetList(parameters, scope, CancellationToken.None);
        var result = task.GetAwaiter().GetResult();

        return result;
    }

    protected override async Task<IReadOnlyCollection<TDomainEntity>> DoGetList(IReadOnlyCollection<FilterCriteria>? parameters, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var query = scope.Advanced.AsyncDocumentQuery<TDocument>().ApplyFilter(parameters);
        var documents = await query.ToListAsync(cancellationToken);
        var entities = MapToEntityCollection(documents);

        return entities;
    }
}
