using System;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using YuckQi.Data.Sql.EntityFramework;

namespace YuckQi.Data.Sql.EntityFramework.UnitTests;

public class UnitOfWorkTests
{
    [Test]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new UnitOfWork<TestDbContext>(null!));
    }

    [Test]
    public void Constructor_WithContext_CreatesInstance()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("UnitOfWork_Constructor").Options;
        using var context = new TestDbContext(options);

        var uow = new UnitOfWork<TestDbContext>(context);

        Assert.That(uow, Is.Not.Null);
        Assert.That(uow.Scope, Is.SameAs(context));
    }

    [Test]
    public void SaveChanges_CallsContextSaveChanges()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("UnitOfWork_SaveChanges").Options;
        using var context = new TestDbContext(options);
        var uow = new UnitOfWork<TestDbContext>(context);

        uow.SaveChanges();

        Assert.DoesNotThrow(() => uow.SaveChanges());
    }

    [Test]
    public async Task SaveChanges_WithCancellationToken_CallsContextSaveChangesAsync()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("UnitOfWork_SaveChanges_Async").Options;
        await using var context = new TestDbContext(options);
        var uow = new UnitOfWork<TestDbContext>(context);

        await uow.SaveChanges(CancellationToken.None);

        Assert.Pass();
    }

    [Test]
    public void Dispose_DisposesContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("UnitOfWork_Dispose").Options;
        var context = new TestDbContext(options);
        var uow = new UnitOfWork<TestDbContext>(context);

        uow.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _ = context.Set<SurLaTableRecord>().Count());
    }
}
