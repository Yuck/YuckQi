using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace YuckQi.Data.UnitTests.Sql;

public sealed class FakeDbConnection : IDbConnection
{
    private readonly Object _lock = new();

    public FakeDbConnection(ConnectionState state = ConnectionState.Closed)
    {
        State = state;
    }

    public List<String> Calls { get; } = [];

    public Exception? CommitFailure { get; set; }

    [AllowNull]
    public String ConnectionString { get; set; } = String.Empty;

    public Int32 ConnectionTimeout => 0;

    public String Database => "fake";

    public Exception? RollbackFailure { get; set; }

    public ConnectionState State { get; private set; }

    public Exception? TransactionDisposeFailure { get; set; }

    public IDbTransaction BeginTransaction()
    {
        return BeginTransaction(IsolationLevel.ReadCommitted);
    }

    public IDbTransaction BeginTransaction(IsolationLevel il)
    {
        if (State != ConnectionState.Open)
            throw new InvalidOperationException();

        Record($"Begin:{il}");

        return new FakeDbTransaction(this, il);
    }

    public void ChangeDatabase(String databaseName)
    {
        throw new NotSupportedException();
    }

    public void Close()
    {
        Record("Close");

        State = ConnectionState.Closed;
    }

    public IDbCommand CreateCommand()
    {
        throw new NotSupportedException();
    }

    public void Dispose()
    {
        Record("Dispose:Connection");

        State = ConnectionState.Closed;
    }

    public void Open()
    {
        Record("Open");

        State = ConnectionState.Open;
    }

    public void Record(String call)
    {
        lock (_lock)
            Calls.Add(call);
    }
}
