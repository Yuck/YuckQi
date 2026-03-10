using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Data.Sql.EntityFramework.Handlers.Write;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.Sql.EntityFramework.UnitTests.Handlers.Write;

public class RevisionHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new RevisionHandler<SurLaTableRecord, Int32, TestDbContext>(null);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void ConstructorWithOptions_CreatesInstance()
    {
        var options = new RevisionOptions(PropertyHandling.Auto);
        var handler = new RevisionHandler<SurLaTableRecord, Int32, TestDbContext>(options);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void MapperConstructor_CreatesInstance()
    {
        var handler = new RevisionHandler<SurLaTableRecord, Int32, TestDbContext, SurLaTableRecord>(null, new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Revise_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RevisionHandler<SurLaTableRecord, Int32, TestDbContext>(null);
        var entity = new SurLaTableRecord { Identifier = 1, Name = "ABC", RevisionMoment = DateTimeOffset.UtcNow };

        Assert.Throws<ArgumentNullException>(() => handler.Revise(entity, null));
    }

    [Test]
    public void ReviseAsync_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RevisionHandler<SurLaTableRecord, Int32, TestDbContext>(null);
        var entity = new SurLaTableRecord { Identifier = 1, Name = "ABC", RevisionMoment = DateTimeOffset.UtcNow };

        Assert.ThrowsAsync<ArgumentNullException>(() => handler.Revise(entity, null, CancellationToken.None));
    }

    [Test]
    public void Revise_WithScope_UpdatesEntityAndReturnsIt()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var context = new TestDbContext(options);
        var entity = new SurLaTableRecord { Id = 1, Name = "Old", RevisionMoment = DateTimeOffset.UtcNow };
        var handler = new RevisionHandler<SurLaTableRecord, Int32, TestDbContext>(null);

        context.SurLaTable.Add(entity);
        context.SaveChanges();
        entity.Name = "New";

        var result = handler.Revise(entity, context);

        Assert.That(result, Is.EqualTo(entity));
        var record = context.SurLaTable.Find(1);
        Assert.That(record!.Name, Is.EqualTo("New"));
    }

    [Test]
    public async Task ReviseAsync_WithScope_UpdatesEntityAndReturnsIt()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var context = new TestDbContext(options);
        var entity = new SurLaTableRecord { Id = 1, Name = "Old", RevisionMoment = DateTimeOffset.UtcNow };
        var handler = new RevisionHandler<SurLaTableRecord, Int32, TestDbContext>(null);

        context.SurLaTable.Add(entity);
        await context.SaveChangesAsync(CancellationToken.None);
        entity.Name = "Updated";

        var result = await handler.Revise(entity, context, CancellationToken.None);

        Assert.That(result, Is.EqualTo(entity));
        var record = await context.SurLaTable.FindAsync([1], CancellationToken.None);
        Assert.That(record!.Name, Is.EqualTo("Updated"));
    }
}
