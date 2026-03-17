using System.Collections.Concurrent;
using NUnit.Framework;
using YuckQi.Data.Filtering;
using YuckQi.Data.MemDb.Handlers.Read;
using YuckQi.Data.MemDb.Handlers.Write;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract;

namespace YuckQi.Data.MemDb.UnitTests.Handlers.Read;

public class RetrievalHandlerTests
{
    [Test]
    public void Get_ByIdentifier_ReturnsEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var retriever = new RetrievalHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        var created = creator.Create(entity, scope);
        var retrieved = retriever.Get(created.Identifier, scope);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entities.Values.ToList(), Does.Contain(created));
            Assert.That(retrieved?.Identifier, Is.EqualTo(created.Identifier));
        }
    }

    [Test]
    public void Get_ByObject_WhenMatch_ReturnsEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var retriever = new RetrievalHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        var created = creator.Create(entity, scope);
        var parameters = new { Name = "ABC" };
        var retrieved = retriever.Get(parameters, scope);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entities.Values.ToList(), Does.Contain(created));
            Assert.That(retrieved?.Identifier, Is.EqualTo(created.Identifier));
        }
    }

    [Test]
    public void Get_ByObject_WhenNoMatch_ReturnsNull()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var retriever = new RetrievalHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        var created = creator.Create(entity, scope);
        var parameters = new { Name = "ZZZ" };
        var retrieved = retriever.Get(parameters, scope);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entities.Values.ToList(), Does.Contain(created));
            Assert.That(retrieved, Is.Null);
        }
    }

    [Test]
    public void GetList_ByObject_WhenMatch_ContainsEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var retriever = new RetrievalHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        var created = creator.Create(entity, scope);
        var parameters = new { Name = "ABC" };
        var retrieved = retriever.GetList(parameters, scope);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entities.Values.ToList(), Does.Contain(created));
            Assert.That(retrieved, Contains.Item(created));
        }
    }

    [Test]
    public void GetList_ByObject_WhenNoMatch_DoesNotContainEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var retriever = new RetrievalHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        var created = creator.Create(entity, scope);
        var parameters = new { Name = "ZZZ" };
        var retrieved = retriever.GetList(parameters, scope);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entities.Values.ToList(), Does.Contain(created));
            Assert.That(retrieved, Does.Not.Contain(created));
        }
    }

    [Test]
    public void Get_ByIdentifier_NotFound_ReturnsDefault()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var retriever = new RetrievalHandler<SurLaTable, Int32, Object>(entities);

        var retrieved = retriever.Get(999, new Object());

        Assert.That(retrieved, Is.Null);
    }

    [Test]
    public async Task Get_ByIdentifier_Async_ReturnsEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var retriever = new RetrievalHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        var created = creator.Create(entity, scope);
        var retrieved = await retriever.Get(created.Identifier, scope, CancellationToken.None);

        Assert.That(retrieved?.Identifier, Is.EqualTo(created.Identifier));
    }

    [Test]
    public async Task Get_ByFilterCriteria_Async_ReturnsEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var retriever = new RetrievalHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        creator.Create(entity, scope);

        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var retrieved = await retriever.Get(parameters, scope, CancellationToken.None);

        Assert.That(retrieved?.Identifier, Is.EqualTo(1));
    }

    [Test]
    public async Task Get_ByObject_Async_ReturnsEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var retriever = new RetrievalHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        creator.Create(entity, scope);

        var retrieved = await retriever.Get(new { Name = "ABC" }, scope, CancellationToken.None);

        Assert.That(retrieved?.Identifier, Is.EqualTo(1));
    }

    [Test]
    public void Get_ByFilterCriteria_ReturnsEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var retriever = new RetrievalHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        creator.Create(entity, scope);

        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var retrieved = retriever.Get(parameters, scope);

        Assert.That(retrieved?.Identifier, Is.EqualTo(1));
    }

    [Test]
    public void GetList_NoParameters_ReturnsAll()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var retriever = new RetrievalHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();

        creator.Create(new SurLaTable { Identifier = 1, Name = "ABC" }, scope);
        creator.Create(new SurLaTable { Identifier = 2, Name = "DEF" }, scope);

        var retrieved = retriever.GetList(scope);

        Assert.That(retrieved, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetList_NoParameters_Async_ReturnsAll()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var retriever = new RetrievalHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();

        creator.Create(new SurLaTable { Identifier = 1, Name = "ABC" }, scope);

        var retrieved = await retriever.GetList(scope, CancellationToken.None);

        Assert.That(retrieved, Has.Count.EqualTo(1));
    }

    [Test]
    public void GetList_ByFilterCriteria_ReturnsMatchingEntities()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var retriever = new RetrievalHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();

        creator.Create(new SurLaTable { Identifier = 1, Name = "ABC" }, scope);
        creator.Create(new SurLaTable { Identifier = 2, Name = "DEF" }, scope);

        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var retrieved = retriever.GetList(parameters, scope);

        Assert.That(retrieved, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetList_ByFilterCriteria_Async_ReturnsMatchingEntities()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var retriever = new RetrievalHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();

        creator.Create(new SurLaTable { Identifier = 1, Name = "ABC" }, scope);

        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var retrieved = await retriever.GetList(parameters, scope, CancellationToken.None);

        Assert.That(retrieved, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetList_ByObject_Async_ReturnsMatchingEntities()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var retriever = new RetrievalHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();

        creator.Create(new SurLaTable { Identifier = 1, Name = "ABC" }, scope);

        var retrieved = await retriever.GetList(new { Name = "ABC" }, scope, CancellationToken.None);

        Assert.That(retrieved, Has.Count.EqualTo(1));
    }

    public record SurLaTable : DomainEntityBase<Int32>, ICreationMoment
    {
        public DateTimeOffset CreationMoment { get; set; }

        public String Name { get; set; } = String.Empty;
    }
}
