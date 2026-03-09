using System;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Data.Sql.EntityFramework.Handlers.Write;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.Sql.EntityFramework.UnitTests.Handlers.Write;

public class CreationHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new CreationHandler<SurLaTableRecord, Int32, TestDbContext>();

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void MapperConstructor_CreatesInstance()
    {
        var handler = new CreationHandler<SurLaTableRecord, Int32, TestDbContext, SurLaTableRecord>(null, new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Create_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new CreationHandler<SurLaTableRecord, Int32, TestDbContext>();
        var entity = new SurLaTableRecord { Identifier = 1, Name = "ABC", CreationMoment = DateTimeOffset.UtcNow };

        Assert.Throws<ArgumentNullException>(() => handler.Create(entity, null));
    }

    [Test]
    public void CreateAsync_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new CreationHandler<SurLaTableRecord, Int32, TestDbContext>();
        var entity = new SurLaTableRecord { Identifier = 1, Name = "ABC", CreationMoment = DateTimeOffset.UtcNow };

        Assert.ThrowsAsync<ArgumentNullException>(() => handler.Create(entity, null, CancellationToken.None));
    }

    [Test]
    public void Create_WithScope_AddsEntityAndReturnsIdentifier()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var context = new TestDbContext(options);
        var handler = new CreationHandler<SurLaTableRecord, Int32, TestDbContext>();
        var entity = new SurLaTableRecord { Id = 1, Name = "ABC", CreationMoment = DateTimeOffset.UtcNow };

        var result = handler.Create(entity, context);
        context.SaveChanges();

        Assert.That(result.Identifier, Is.EqualTo(1));
        Assert.That(context.ChangeTracker.Entries<SurLaTableRecord>().Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task CreateAsync_WithScope_AddsEntityAndReturnsIdentifier()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var context = new TestDbContext(options);
        var handler = new CreationHandler<SurLaTableRecord, Int32, TestDbContext>();
        var entity = new SurLaTableRecord { Id = 1, Name = "XYZ", CreationMoment = DateTimeOffset.UtcNow };

        var result = await handler.Create(entity, context, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        Assert.That(result.Identifier, Is.EqualTo(1));
        Assert.That(context.ChangeTracker.Entries<SurLaTableRecord>().Count(), Is.EqualTo(1));
    }

    [Test]
    public void Create_WithOptionsAndIdentifierFactory_SetsIdentifierFromFactory()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("Creation_WithFactory").Options;
        using var context = new TestDbContext(options);
        var creationOptions = new CreationOptions<Int32>(() => 42);
        var handler = new CreationHandler<SurLaTableRecord, Int32, TestDbContext>(creationOptions);
        var entity = new SurLaTableRecord { Identifier = 0, Name = "Test", CreationMoment = DateTimeOffset.UtcNow };

        var result = handler.Create(entity, context);

        Assert.That(result.Identifier, Is.EqualTo(42));
    }
}
