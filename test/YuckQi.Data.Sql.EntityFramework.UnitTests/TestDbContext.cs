using Microsoft.EntityFrameworkCore;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;

namespace YuckQi.Data.Sql.EntityFramework.UnitTests;

internal class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<ManualKeyRecord> ManualKey => Set<ManualKeyRecord>();

    public DbSet<PersonRecord> People => Set<PersonRecord>();

    public DbSet<SurLaTableRecord> SurLaTable => Set<SurLaTableRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ManualKeyRecord>(t =>
        {
            t.HasKey(u => u.Id);
            t.Property(u => u.Id).ValueGeneratedNever();
            t.Ignore(u => u.Identifier);
        });
        modelBuilder.Entity<PersonRecord>(t =>
        {
            t.HasKey(u => u.Id);
            t.Property(u => u.Id).ValueGeneratedNever();
            t.OwnsOne(u => u.Address);
        });
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
