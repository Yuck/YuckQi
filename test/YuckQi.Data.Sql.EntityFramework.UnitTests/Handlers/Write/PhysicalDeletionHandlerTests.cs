using System;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using YuckQi.Data.Sql.EntityFramework.Handlers.Write;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.Sql.EntityFramework.UnitTests.Handlers.Write;

public class PhysicalDeletionHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new PhysicalDeletionHandler<SurLaTableRecord, Int32, TestDbContext>();

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void MapperConstructor_CreatesInstance()
    {
        var handler = new PhysicalDeletionHandler<SurLaTableRecord, Int32, TestDbContext, SurLaTableRecord>(new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Delete_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new PhysicalDeletionHandler<SurLaTableRecord, Int32, TestDbContext>();
        var entity = new SurLaTableRecord { Identifier = 1, Name = "ABC" };

        Assert.Throws<ArgumentNullException>(() => handler.Delete(entity, null));
    }

    [Test]
    public void DeleteAsync_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new PhysicalDeletionHandler<SurLaTableRecord, Int32, TestDbContext>();
        var entity = new SurLaTableRecord { Identifier = 1, Name = "ABC" };

        Assert.ThrowsAsync<ArgumentNullException>(() => handler.Delete(entity, null, CancellationToken.None));
    }

    [Test]
    public void Delete_WithScope_RemovesEntityAndReturnsIt()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var context = new TestDbContext(options);
        var entity = new SurLaTableRecord { Id = 1, Name = "ToDelete" };
        var handler = new PhysicalDeletionHandler<SurLaTableRecord, Int32, TestDbContext>();

        context.SurLaTable.Add(entity);
        context.SaveChanges();

        var result = handler.Delete(entity, context);

        context.SaveChanges();

        Assert.That(result, Is.EqualTo(entity));
        Assert.That(context.SurLaTable.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteAsync_WithScope_RemovesEntityAndReturnsIt()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var context = new TestDbContext(options);
        var entity = new SurLaTableRecord { Id = 1, Name = "ToDelete" };
        var handler = new PhysicalDeletionHandler<SurLaTableRecord, Int32, TestDbContext>();

        context.SurLaTable.Add(entity);
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await handler.Delete(entity, context, CancellationToken.None);

        await context.SaveChangesAsync(CancellationToken.None);

        Assert.That(result, Is.EqualTo(entity));
        Assert.That(await context.SurLaTable.CountAsync(CancellationToken.None), Is.EqualTo(0));
    }
}
