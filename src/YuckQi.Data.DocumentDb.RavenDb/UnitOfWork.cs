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
        if (_session == null)
            return;

        _session.Dispose();

        _session = null;
    }

    public void SaveChanges()
    {
        if (_session == null)
            throw new InvalidOperationException();

        _session.SaveChangesAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    private static IAsyncDocumentSession OpenSession(IDocumentStore store, String? database)
    {
        return database != null ? store.OpenAsyncSession(database) : store.OpenAsyncSession();
    }
}
