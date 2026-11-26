using System.Data;
using System.Data.Common;
using YuckQi.Data.Abstract.Interfaces;

namespace YuckQi.Data.Sql;

public class UnitOfWork<TScope, TDbConnection> : IUnitOfWork<TScope> where TScope : class, IDbTransaction where TDbConnection : class, IDbConnection
{
    private TDbConnection? _connection;
    private readonly IsolationLevel _isolation;
    private readonly Object _lock = new();
    private TScope? _transaction;

    public UnitOfWork(TDbConnection connection, IsolationLevel isolation = IsolationLevel.ReadCommitted)
    {
        ArgumentNullException.ThrowIfNull(connection);

        _connection = connection;
        _isolation = isolation;
    }

    public TScope Scope
    {
        get
        {
            lock (_lock)
            {
                if (_transaction is not null)
                    return _transaction;

                _transaction = StartTransaction();

                return _transaction;
            }
        }
    }

    public void Dispose()
    {
        var (transaction, connection) = DetachResources();

        if (connection is null)
            return;

        try
        {
            if (transaction is not null)
                RollBack(transaction);
        }
        finally
        {
            CloseConnection(connection);
        }
    }

    public void SaveChanges()
    {
        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_connection is null, this);

            if (_transaction is null)
                return;

            var transaction = _transaction;

            _transaction = null;

            try
            {
                transaction.Commit();
            }
            catch (Exception)
            {
                TryDispose(transaction);

                throw;
            }

            transaction.Dispose();
        }
    }

    public Task SaveChanges(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        SaveChanges();

        return Task.CompletedTask;
    }

    private static void CloseConnection(TDbConnection connection)
    {
        try
        {
            connection.Close();
        }
        finally
        {
            connection.Dispose();
        }
    }

    private (TScope? Transaction, TDbConnection? Connection) DetachResources()
    {
        lock (_lock)
        {
            var detached = (_transaction, _connection);

            _transaction = null;
            _connection = null;

            return detached;
        }
    }

    private static Boolean IsCleanupFailure(Exception exception)
    {
        return exception is DbException or InvalidOperationException;
    }

    private static void RollBack(TScope transaction)
    {
        try
        {
            transaction.Rollback();
        }
        catch (Exception exception) when (IsCleanupFailure(exception))
        {
        }
        finally
        {
            TryDispose(transaction);
        }
    }

    private TScope StartTransaction()
    {
        ObjectDisposedException.ThrowIf(_connection is null, this);

        if (_connection.State == ConnectionState.Closed)
            _connection.Open();

        return (TScope) _connection.BeginTransaction(_isolation);
    }

    private static void TryDispose(TScope transaction)
    {
        try
        {
            transaction.Dispose();
        }
        catch (Exception exception) when (IsCleanupFailure(exception))
        {
        }
    }
}
