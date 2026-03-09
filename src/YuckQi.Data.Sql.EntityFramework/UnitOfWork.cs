using Microsoft.EntityFrameworkCore;
using YuckQi.Data.Abstract.Interfaces;

namespace YuckQi.Data.Sql.EntityFramework;

public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
{
    private readonly TContext _context;

    public TContext Scope => _context;

    public UnitOfWork(TContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }

    public Task SaveChanges(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
