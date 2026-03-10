using Microsoft.EntityFrameworkCore;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;

namespace YuckQi.Data.Sql.EntityFramework.UnitTests;

internal class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<SurLaTableRecord> SurLaTable => Set<SurLaTableRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SurLaTableRecord>(t =>
        {
            t.HasKey(u => u.Id);
            t.Property(u => u.Name).IsRequired();
        });
    }
}

internal class SurLaTableRecord : IDomainEntity<Int32>, ICreationMoment, IRevisionMoment
{
    public DateTimeOffset CreationMoment { get; set; }

    public Int32 Id { get; set; }

    public Int32 Identifier { get => Id; set => Id = value; }

    public String Name { get; set; } = String.Empty;

    public DateTimeOffset RevisionMoment { get; set; }
}
