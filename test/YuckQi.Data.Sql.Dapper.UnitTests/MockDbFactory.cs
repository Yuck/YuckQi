using System.Data;
using System.Data.Common;
using Moq;
using Moq.Protected;

namespace YuckQi.Data.Sql.Dapper.UnitTests;

internal static class MockDbFactory
{
    public static IDbTransaction CreateTransaction(ICollection<String>? commandTexts = null)
    {
        var connection = CreateConnection(commandTexts);
        var transaction = new Mock<DbTransaction>();

        transaction.Protected().SetupGet<DbConnection>("DbConnection").Returns(connection);
        transaction.Setup(t => t.IsolationLevel).Returns(IsolationLevel.ReadCommitted);

        return transaction.Object;
    }

    public static DbConnection CreateConnection(ICollection<String>? commandTexts = null)
    {
        var connection = new Mock<DbConnection>();

        connection.Setup(t => t.State).Returns(ConnectionState.Open);
        connection.Setup(t => t.Database).Returns("mock");
        connection.Setup(t => t.DataSource).Returns("mock");
        connection.Setup(t => t.ServerVersion).Returns("1.0");
        connection.Protected()
                  .Setup<DbCommand>("CreateDbCommand")
                  .Returns(() => CreateCommand(commandTexts));

        return connection.Object;
    }

    private static DbCommand CreateCommand(ICollection<String>? commandTexts)
    {
        var parameters = new List<DbParameter>();
        var parameterCollection = new Mock<DbParameterCollection>();

        parameterCollection.Setup(t => t.Add(It.IsAny<Object>()))
                           .Callback<Object>(t => parameters.Add((DbParameter) t))
                           .Returns(() => parameters.Count - 1);
        parameterCollection.Setup(t => t.Count).Returns(() => parameters.Count);
        parameterCollection.Setup(t => t.Contains(It.IsAny<String>()))
                           .Returns<String>(t => parameters.Any(u => u.ParameterName == t));
        parameterCollection.Setup(t => t.IndexOf(It.IsAny<String>()))
                           .Returns<String>(t => parameters.FindIndex(u => u.ParameterName == t));
        parameterCollection.Setup(t => t.GetEnumerator()).Returns(() => parameters.GetEnumerator());
        parameterCollection.Setup(t => t.SyncRoot).Returns(new Object());

        var command = new Mock<DbCommand>();

        command.SetupAllProperties();
        command.Protected()
               .SetupGet<DbParameterCollection>("DbParameterCollection")
               .Returns(parameterCollection.Object);
        command.Protected()
               .Setup<DbParameter>("CreateDbParameter")
               .Returns(() => CreateParameter());
        command.Setup(t => t.ExecuteNonQuery()).Callback(Record).Returns(1);
        command.Setup(t => t.ExecuteScalar()).Callback(Record).Returns(0);
        command.Setup(t => t.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Callback(Record).ReturnsAsync(1);
        command.Setup(t => t.ExecuteScalarAsync(It.IsAny<CancellationToken>())).Callback(Record).Returns(Task.FromResult<Object?>(0));
        command.Protected()
               .Setup<DbDataReader>("ExecuteDbDataReader", ItExpr.IsAny<CommandBehavior>())
               .Callback(Record)
               .Returns(() => CreateDataReader());
        command.Protected()
               .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
               .Callback(Record)
               .Returns(() => Task.FromResult(CreateDataReader()));

        return command.Object;

        void Record()
        {
            commandTexts?.Add(command.Object.CommandText);
        }
    }

    private static DbParameter CreateParameter()
    {
        var parameter = new Mock<DbParameter>();

        parameter.SetupAllProperties();

        return parameter.Object;
    }

    private static DbDataReader CreateDataReader()
    {
        var reader = new Mock<DbDataReader>();

        reader.Setup(t => t.Read()).Returns(false);
        reader.Setup(t => t.ReadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        reader.Setup(t => t.NextResult()).Returns(false);
        reader.Setup(t => t.NextResultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        reader.Setup(t => t.FieldCount).Returns(0);
        reader.Setup(t => t.HasRows).Returns(false);
        reader.Setup(t => t.IsClosed).Returns(false);
        reader.Setup(t => t.Depth).Returns(0);
        reader.Setup(t => t.RecordsAffected).Returns(-1);
        reader.Setup(t => t.GetEnumerator()).Returns(Enumerable.Empty<Object>().GetEnumerator());

        return reader.Object;
    }
}
