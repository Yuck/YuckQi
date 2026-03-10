using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using YuckQi.Data.Filtering;
using YuckQi.Data.Sql.EntityFramework.Handlers.Read;

namespace YuckQi.Data.Sql.EntityFramework.UnitTests.Handlers.Read;

public class RetrievalHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new RetrievalHandler<SurLaTableRecord, Int32, TestDbContext>();

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Get_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableRecord, Int32, TestDbContext>();

        Assert.Throws<ArgumentNullException>(() => handler.Get(1, null));
    }

    [Test]
    public void GetAsync_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableRecord, Int32, TestDbContext>();

        Assert.ThrowsAsync<ArgumentNullException>(() => handler.Get(1, null, CancellationToken.None));
    }

    [Test]
    public void Get_ByIdentifier_WhenNotExists_ReturnsNull()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("Retrieval_Get_NotExists").Options;
        using var context = new TestDbContext(options);
        var handler = new RetrievalHandler<SurLaTableRecord, Int32, TestDbContext>();

        var result = handler.Get(1, context);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAsync_ByIdentifier_WhenNotExists_ReturnsNull()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("Retrieval_GetAsync_NotExists").Options;
        await using var context = new TestDbContext(options);
        var handler = new RetrievalHandler<SurLaTableRecord, Int32, TestDbContext>();

        var result = await handler.Get(1, context, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Get_ByIdentifier_WhenExists_ReturnsEntity()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("Retrieval_Get_Exists").Options;
        using var context = new TestDbContext(options);
        var record = new SurLaTableRecord { Id = 1, Name = "ABC" };
        var handler = new RetrievalHandler<SurLaTableRecord, Int32, TestDbContext>();

        context.SurLaTable.Add(record);
        context.SaveChanges();

        var result = handler.Get(1, context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Identifier, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("ABC"));
    }

    [Test]
    public async Task GetAsync_ByIdentifier_WhenExists_ReturnsEntity()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("Retrieval_GetAsync_Exists").Options;
        await using var context = new TestDbContext(options);
        var record = new SurLaTableRecord { Id = 1, Name = "XYZ" };
        var handler = new RetrievalHandler<SurLaTableRecord, Int32, TestDbContext>();

        context.SurLaTable.Add(record);
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await handler.Get(1, context, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Identifier, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("XYZ"));
    }

    [Test]
    public void Get_ByFilterCriteria_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableRecord, Int32, TestDbContext>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        Assert.Throws<ArgumentNullException>(() => handler.Get(parameters, null));
    }

    [Test]
    public void GetAsync_ByFilterCriteria_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableRecord, Int32, TestDbContext>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        Assert.ThrowsAsync<ArgumentNullException>(() => handler.Get(parameters, null, CancellationToken.None));
    }

    [Test]
    public void Get_ByFilterCriteria_WhenMatchExists_ReturnsEntity()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("Retrieval_Get_Filter").Options;
        using var context = new TestDbContext(options);
        var record = new SurLaTableRecord { Id = 1, Name = "ABC" };
        var handler = new RetrievalHandler<SurLaTableRecord, Int32, TestDbContext>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        context.SurLaTable.Add(record);
        context.SaveChanges();

        var result = handler.Get(parameters, context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("ABC"));
    }

    [Test]
    public void GetList_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableRecord, Int32, TestDbContext>();

        Assert.Throws<ArgumentNullException>(() => handler.GetList(null));
    }

    [Test]
    public void GetListAsync_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableRecord, Int32, TestDbContext>();

        Assert.ThrowsAsync<ArgumentNullException>(() => handler.GetList(null, CancellationToken.None));
    }

    [Test]
    public void GetList_WithScope_ReturnsMatchingEntities()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("Retrieval_GetList").Options;
        using var context = new TestDbContext(options);
        var handler = new RetrievalHandler<SurLaTableRecord, Int32, TestDbContext>();

        context.SurLaTable.Add(new SurLaTableRecord { Id = 1, Name = "A" });
        context.SurLaTable.Add(new SurLaTableRecord { Id = 2, Name = "B" });
        context.SaveChanges();

        var result = handler.GetList(context);

        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public void GetList_ByFilterCriteria_ReturnsMatchingEntities()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("Retrieval_GetList_Filter").Options;
        using var context = new TestDbContext(options);
        var handler = new RetrievalHandler<SurLaTableRecord, Int32, TestDbContext>();
        var parameters = new[] { new FilterCriteria("Name", "Match") };

        context.SurLaTable.Add(new SurLaTableRecord { Id = 1, Name = "Match" });
        context.SurLaTable.Add(new SurLaTableRecord { Id = 2, Name = "Other" });
        context.SaveChanges();

        var result = handler.GetList(parameters, context);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("Match"));
    }

    [Test]
    public void Get_ByFilterCriteria_WithGreaterThan_ReturnsMatchingEntity()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("Retrieval_Get_GreaterThan").Options;
        using var context = new TestDbContext(options);
        var handler = new RetrievalHandler<SurLaTableRecord, Int32, TestDbContext>();
        var parameters = new[] { new FilterCriteria("Id", FilterOperation.GreaterThan, 1) };

        context.SurLaTable.Add(new SurLaTableRecord { Id = 1, Name = "A" });
        context.SurLaTable.Add(new SurLaTableRecord { Id = 2, Name = "B" });
        context.SurLaTable.Add(new SurLaTableRecord { Id = 3, Name = "C" });
        context.SaveChanges();

        var result = handler.Get(parameters, context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Identifier, Is.GreaterThan(1));
    }
}
