using System.Data;
using YuckQi.Data.Abstract.Interfaces;

namespace YuckQi.Data.Sql;

public class UnitOfWork<TScope, TDbConnection> : IUnitOfWork<TScope> where TScope : class, IDbTransaction where TDbConnection : class, IDbConnection
{
    private TDbConnection? _connection;
    private readonly IsolationLevel _isolation;
    private readonly Object _lock = new();
    private Lazy<TScope>? _transaction;

    public UnitOfWork(TDbConnection connection, IsolationLevel isolation = IsolationLevel.ReadCommitted)
    {
        ArgumentNullException.ThrowIfNull(connection);

        _connection = connection;
        _isolation = isolation;
        _transaction = new Lazy<TScope>(StartTransaction);
    }

    public TScope Scope => _transaction is not null ? _transaction.Value : throw new InvalidOperationException();

    public void Dispose()
    {
        if (_transaction is not null)
        {
            Scope.Rollback();
            Scope.Dispose();

            _transaction = null;
        }

        if (_connection is null)
            return;

        _connection.Close();
        _connection.Dispose();

        _connection = null;
    }

    public void SaveChanges()
    {
        lock (_lock)
        {
            if (_transaction is null)
                throw new InvalidOperationException();

            Scope.Commit();
            Scope.Dispose();

            _transaction = new Lazy<TScope>(StartTransaction);
        }
    }

    public Task SaveChanges(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        SaveChanges();

        return Task.CompletedTask;
    }

    private TScope StartTransaction()
    {
        lock (_lock)
        {
            if (_connection is { State: ConnectionState.Closed })
                _connection.Open();

            return _connection is not null ? (TScope) _connection.BeginTransaction(_isolation) : throw new InvalidOperationException();
        }
    }
}
