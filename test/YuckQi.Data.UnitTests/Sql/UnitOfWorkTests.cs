using System.Data;
using NUnit.Framework;
using YuckQi.Data.Sql;

namespace YuckQi.Data.UnitTests.Sql;

public class UnitOfWorkTests
{
    private static readonly String[] DatabaseCalls = ["Open", "Begin", "Commit", "Rollback"];

    [Test]
    public void Constructor_DoesNotOpenConnectionOrStartTransaction()
    {
        var connection = new FakeDbConnection();

        _ = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);

        Assert.That(connection.Calls, Is.Empty);
    }

    [Test]
    public void Dispose_AfterSaveChanges_IssuesNoDatabaseCalls()
    {
        var connection = new FakeDbConnection();
        var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);

        _ = uow.Scope;
        uow.SaveChanges();

        var before = connection.Calls.Count;

        uow.Dispose();

        Assert.That(connection.Calls.Skip(before), Is.EqualTo(new[] { "Close", "Dispose:Connection" }));
        Assert.That(connection.Calls.Count(t => DatabaseCalls.Any(t.StartsWith)), Is.EqualTo(3));
    }

    [Test]
    public void Dispose_CalledTwice_ClosesConnectionOnce()
    {
        var connection = new FakeDbConnection();
        var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);

        _ = uow.Scope;
        uow.Dispose();
        uow.Dispose();

        Assert.That(connection.Calls.Count(t => t == "Dispose:Connection"), Is.EqualTo(1));
        Assert.That(connection.Calls.Count(t => t == "Rollback"), Is.EqualTo(1));
    }

    [Test]
    public void Dispose_WhenRollbackThrowsDbException_DoesNotThrowAndClosesConnection()
    {
        var connection = new FakeDbConnection { RollbackFailure = new FakeDbException("Rollback failed.") };
        var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);

        _ = uow.Scope;

        Assert.DoesNotThrow(() => uow.Dispose());
        Assert.That(connection.Calls, Is.EqualTo(new[] { "Open", "Begin:ReadCommitted", "Rollback", "Dispose:Transaction", "Close", "Dispose:Connection" }));
    }

    [Test]
    public void Dispose_WhenRollbackThrowsUnexpectedException_ThrowsAndStillDisposesTransactionAndConnection()
    {
        var connection = new FakeDbConnection { RollbackFailure = new NotSupportedException("Rollback failed.") };
        var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);

        _ = uow.Scope;

        Assert.Throws<NotSupportedException>(() => uow.Dispose());
        Assert.That(connection.Calls, Is.EqualTo(new[] { "Open", "Begin:ReadCommitted", "Rollback", "Dispose:Transaction", "Close", "Dispose:Connection" }));
    }

    [Test]
    public void Dispose_WhenRollbackThrows_DoesNotMaskOriginalException()
    {
        var connection = new FakeDbConnection { RollbackFailure = new InvalidOperationException("Rollback failed.") };

        var exception = Assert.Throws<ApplicationException>(() =>
        {
            using var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);

            _ = uow.Scope;

            throw new ApplicationException("Original failure.");
        });

        Assert.That(exception?.Message, Is.EqualTo("Original failure."));
        Assert.That(connection.Calls, Does.Contain("Dispose:Connection"));
    }

    [Test]
    public void Dispose_WhenRollbackThrows_DoesNotThrowAndClosesConnection()
    {
        var connection = new FakeDbConnection { RollbackFailure = new InvalidOperationException("Rollback failed.") };
        var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);

        _ = uow.Scope;

        Assert.DoesNotThrow(() => uow.Dispose());
        Assert.That(connection.Calls, Is.EqualTo(new[] { "Open", "Begin:ReadCommitted", "Rollback", "Dispose:Transaction", "Close", "Dispose:Connection" }));
        Assert.That(connection.State, Is.EqualTo(ConnectionState.Closed));
    }

    [Test]
    public void Dispose_WhenTransactionDisposeThrows_DoesNotThrowAndClosesConnection()
    {
        var connection = new FakeDbConnection { TransactionDisposeFailure = new InvalidOperationException("Dispose failed.") };
        var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);

        _ = uow.Scope;

        Assert.DoesNotThrow(() => uow.Dispose());
        Assert.That(connection.Calls, Is.EqualTo(new[] { "Open", "Begin:ReadCommitted", "Rollback", "Dispose:Transaction", "Close", "Dispose:Connection" }));
    }

    [Test]
    public void Dispose_WhenUnused_IssuesNoDatabaseCalls()
    {
        var connection = new FakeDbConnection();
        var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);

        uow.Dispose();

        Assert.That(connection.Calls, Is.EqualTo(new[] { "Close", "Dispose:Connection" }));
    }

    [Test]
    public void Dispose_WithActiveTransaction_RollsBackThenClosesConnection()
    {
        var connection = new FakeDbConnection();
        var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);

        _ = uow.Scope;
        uow.Dispose();

        Assert.That(connection.Calls, Is.EqualTo(new[] { "Open", "Begin:ReadCommitted", "Rollback", "Dispose:Transaction", "Close", "Dispose:Connection" }));
    }

    [Test]
    public void SaveChanges_AfterDispose_ThrowsObjectDisposedException()
    {
        var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(new FakeDbConnection());

        uow.Dispose();

        Assert.Throws<ObjectDisposedException>(() => uow.SaveChanges());
    }

    [Test]
    public void SaveChanges_CommitsAndDisposesTransaction()
    {
        var connection = new FakeDbConnection();
        using var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);

        _ = uow.Scope;
        uow.SaveChanges();

        Assert.That(connection.Calls, Is.EqualTo(new[] { "Open", "Begin:ReadCommitted", "Commit", "Dispose:Transaction" }));
    }

    [Test]
    public void SaveChanges_WhenCommitAndTransactionDisposeThrow_ThrowsCommitException()
    {
        var connection = new FakeDbConnection
        {
            CommitFailure = new FakeDbException("Commit failed."),
            TransactionDisposeFailure = new InvalidOperationException("Dispose failed.")
        };
        using var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);

        _ = uow.Scope;

        var exception = Assert.Throws<FakeDbException>(() => uow.SaveChanges());

        Assert.That(exception?.Message, Is.EqualTo("Commit failed."));
    }

    [Test]
    public void SaveChanges_WhenCommitThrows_DisposesTransactionAndDisposeDoesNotRollBack()
    {
        var connection = new FakeDbConnection { CommitFailure = new InvalidOperationException("Commit failed.") };
        var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);

        _ = uow.Scope;

        Assert.Throws<InvalidOperationException>(() => uow.SaveChanges());

        uow.Dispose();

        Assert.That(connection.Calls, Is.EqualTo(new[] { "Open", "Begin:ReadCommitted", "Commit", "Dispose:Transaction", "Close", "Dispose:Connection" }));
    }

    [Test]
    public async Task SaveChanges_WithCancellationToken_Commits()
    {
        var connection = new FakeDbConnection();
        using var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);

        _ = uow.Scope;
        await uow.SaveChanges(CancellationToken.None);

        Assert.That(connection.Calls, Does.Contain("Commit"));
    }

    [Test]
    public void SaveChanges_WithCancelledToken_ThrowsOperationCanceledException()
    {
        var connection = new FakeDbConnection();
        using var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);
        using var source = new CancellationTokenSource();

        _ = uow.Scope;
        source.Cancel();

        Assert.Throws<OperationCanceledException>(() => uow.SaveChanges(source.Token));
        Assert.That(connection.Calls, Does.Not.Contain("Commit"));
    }

    [Test]
    public void SaveChanges_WithoutScope_IssuesNoDatabaseCalls()
    {
        var connection = new FakeDbConnection();
        using var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);

        uow.SaveChanges();

        Assert.That(connection.Calls, Is.Empty);
    }

    [Test]
    public void Scope_AfterDispose_ThrowsObjectDisposedException()
    {
        var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(new FakeDbConnection());

        uow.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _ = uow.Scope);
    }

    [Test]
    public void Scope_AndSaveChanges_FromConcurrentThreads_DoNotDeadlock()
    {
        var connection = new FakeDbConnection();
        var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);
        var readers =Enumerable.Range(0, 4).Select(t => Task.Run(() =>
        {
            for (var i = 0; i < 1000; i++)
                _ = uow.Scope;
        }));
        var savers = Enumerable.Range(0, 4).Select(t => Task.Run(() =>
        {
            for (var i = 0; i < 1000; i++)
                uow.SaveChanges();
        }));

        var completed = Task.WhenAll(readers.Concat(savers)).Wait(TimeSpan.FromSeconds(30));

        Assert.That(completed, Is.True);

        uow.Dispose();
    }

    [Test]
    public void Scope_OpensConnectionAndStartsTransactionWithIsolationLevel()
    {
        var connection = new FakeDbConnection();
        using var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection, IsolationLevel.Serializable);

        var scope = uow.Scope;

        Assert.That(scope.IsolationLevel, Is.EqualTo(IsolationLevel.Serializable));
        Assert.That(connection.Calls, Is.EqualTo(new[] { "Open", "Begin:Serializable" }));
    }

    [Test]
    public void Scope_ReturnsSameTransactionUntilSaved()
    {
        var connection = new FakeDbConnection();
        using var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);

        var first = uow.Scope;
        var second = uow.Scope;

        uow.SaveChanges();

        var third = uow.Scope;

        Assert.That(second, Is.SameAs(first));
        Assert.That(third, Is.Not.SameAs(first));
    }

    [Test]
    public void Scope_WhenConnectionIsOpen_DoesNotOpenAgain()
    {
        var connection = new FakeDbConnection(ConnectionState.Open);
        using var uow = new UnitOfWork<FakeDbTransaction, FakeDbConnection>(connection);

        _ = uow.Scope;

        Assert.That(connection.Calls, Is.EqualTo(new[] { "Begin:ReadCommitted" }));
    }
}
