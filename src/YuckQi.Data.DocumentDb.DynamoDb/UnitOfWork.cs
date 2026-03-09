using Amazon.DynamoDBv2.DataModel;
using YuckQi.Data.Abstract.Interfaces;

namespace YuckQi.Data.DocumentDb.DynamoDb;

public class UnitOfWork : IUnitOfWork<IDynamoDBContext>
{
    public IDynamoDBContext? Scope { get; private set; }

    public UnitOfWork(IDynamoDBContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Scope = context;
    }

    public void Dispose()
    {
        if (Scope is null)
            return;

        Scope.Dispose();

        Scope = null;
    }

    public void SaveChanges()
    {
    }

    public Task SaveChanges(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.CompletedTask;
    }
}
