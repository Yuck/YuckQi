using System.Data;

namespace YuckQi.Data.UnitTests.Sql;

public sealed class FakeDbTransaction : IDbTransaction
{
    private readonly FakeDbConnection _connection;

    public FakeDbTransaction(FakeDbConnection connection, IsolationLevel isolationLevel)
    {
        _connection = connection;

        IsolationLevel = isolationLevel;
    }

    public IDbConnection Connection => _connection;

    public IsolationLevel IsolationLevel { get; }

    public void Commit()
    {
        _connection.Record("Commit");

        if (_connection.CommitFailure is not null)
            throw _connection.CommitFailure;
    }

    public void Dispose()
    {
        _connection.Record("Dispose:Transaction");

        if (_connection.TransactionDisposeFailure is not null)
            throw _connection.TransactionDisposeFailure;
    }

    public void Rollback()
    {
        _connection.Record("Rollback");

        if (_connection.RollbackFailure is not null)
            throw _connection.RollbackFailure;
    }
}
