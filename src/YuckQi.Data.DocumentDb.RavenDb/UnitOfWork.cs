using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using YuckQi.Data.Abstract.Interfaces;

namespace YuckQi.Data.DocumentDb.RavenDb;

public class UnitOfWork(IDocumentStore store, String? database = null) : IUnitOfWork<IAsyncDocumentSession>
{
    private IAsyncDocumentSession? _session = OpenSession(store ?? throw new ArgumentNullException(nameof(store)), database);

    public IAsyncDocumentSession? Scope => _session;

    public void Dispose()
    {
        if (_session is null)
            return;

        _session.Dispose();

        _session = null;
    }

    public void SaveChanges()
    {
        SaveChanges(CancellationToken.None).GetAwaiter().GetResult();
    }

    public Task SaveChanges(CancellationToken cancellationToken)
    {
        if (_session is null)
            throw new InvalidOperationException();

        return _session.SaveChangesAsync(cancellationToken);
    }

    private static IAsyncDocumentSession OpenSession(IDocumentStore store, String? database)
    {
        return database is not null ? store.OpenAsyncSession(database) : store.OpenAsyncSession();
    }
}
