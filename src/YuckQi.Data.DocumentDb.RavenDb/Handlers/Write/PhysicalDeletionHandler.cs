using Raven.Client.Documents.Session;
using YuckQi.Data.DocumentDb.RavenDb.Extensions;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.DocumentDb.RavenDb.Handlers.Write;

public class PhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope> : PhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IAsyncDocumentSession?;

public class PhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope, TDocument>(IMapper? mapper = null) : PhysicalDeletionHandlerBase<TDomainEntity, TIdentifier, TScope?, TDocument>(mapper) where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IAsyncDocumentSession?
{
    protected override Boolean DoDelete(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var task = DoDelete(entity, scope, CancellationToken.None);
        var result = task.GetAwaiter().GetResult();

        return result;
    }

    protected override async Task<Boolean> DoDelete(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        cancellationToken.ThrowIfCancellationRequested();

        scope.Delete(entity.Identifier.ToDocumentId());

        return true;
    }
}
