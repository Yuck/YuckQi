using System.Collections.Concurrent;
using System.Text;
using NUnit.Framework;
using YuckQi.Data.Filtering;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Data.MemDb.Handlers.Read;
using YuckQi.Data.MemDb.Handlers.Write;
using YuckQi.Data.Sorting;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract;
using YuckQi.Domain.ValueObjects;

namespace YuckQi.Data.MemDb.UnitTests.Handlers.Read;

public class SearchHandlerTests
{
    [Test]
    public void Search_ByIdentifier_ReturnsMatchingEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        var created = creator.Create(entity, scope);
        var parameters = new { Identifier = 1 };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Descending) }.OrderBy(_ => 1);
        var found = searcher.Search(parameters, page, sort, scope);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entities.Values.ToList(), Does.Contain(created));
            Assert.That(found.Items, Contains.Item(created));
        }
    }

    [Test]
    public void Search_ByName_WhenMatch_ReturnsMatchingEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        var created = creator.Create(entity, scope);
        var parameters = new { Name = "ABC" };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Descending) }.OrderBy(_ => 1);
        var found = searcher.Search(parameters, page, sort, scope);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entities.Values.ToList(), Does.Contain(created));
            Assert.That(found.Items, Contains.Item(created));
        }
    }

    [Test]
    public void Search_ByName_WhenNoMatch_DoesNotContainEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        var created = creator.Create(entity, scope);
        var parameters = new { Name = "ZZZ" };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Descending) }.OrderBy(_ => 1);
        var found = searcher.Search(parameters, page, sort, scope);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entities.Values.ToList(), Does.Contain(created));
            Assert.That(found.Items, Does.Not.Contain(created));
        }
    }

    [Test]
    public void Search_WithLessThanOrEqualFilter_ReturnsPagedResults()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities, new CreationOptions<Int32>(IdentifierFactory));
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        for (var i = 0; i < 50; i++)
            creator.Create(new SurLaTable { Identifier = i, Name = "ABC" }, scope);

        var parameters = new[] { new FilterCriteria("Identifier", FilterOperation.LessThanOrEqual, 25) };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Descending) }.OrderBy(_ => 1);
        var found = searcher.Search(parameters, page, sort, scope);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entities.Count, Is.EqualTo(50));
            Assert.That(found.TotalCount, Is.EqualTo(25));
            Assert.That(found.Items.Count, Is.EqualTo(10));
        }

        return;

        Int32 IdentifierFactory() => entities.Count + 1;
    }

    [Test]
    public void Search_WithDescendingSort_ReturnsSortedResults()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities, new CreationOptions<Int32>(IdentifierFactory));
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        for (var i = 0; i < 50; i++)
            creator.Create(new SurLaTable { Identifier = i, Name = GetRandomName() }, scope);

        Assert.That(entities, Has.Count.EqualTo(50));

        var parameters = new[] { new FilterCriteria("Identifier", FilterOperation.LessThanOrEqual, 25) };
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Name", SortOrder.Descending) }.OrderBy(_ => 1);
        var found = searcher.Search(parameters, page, sort, scope);

        var items = found.Items.ToArray();
        for (var i = 1; i < items.Length; i++)
        {
            var current = items[i];
            var previous = items[i - 1];

            Assert.That(current.Name, Is.LessThanOrEqualTo(previous.Name));
        }

        return;

        Int32 IdentifierFactory() => entities.Count + 1;
    }

    [Test]
    public void Search_WithInFilter_ReturnsSortedResults()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities, new CreationOptions<Int32>(IdentifierFactory));
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        for (var i = 0; i < 50; i++)
            creator.Create(new SurLaTable { Identifier = i, Name = GetRandomName() }, scope);

        Assert.That(entities, Has.Count.EqualTo(50));

        var parameters = new[] { new FilterCriteria("Identifier", FilterOperation.In, new[] { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50 }) };
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Name", SortOrder.Descending) }.OrderBy(_ => 1);
        var found = searcher.Search(parameters, page, sort, scope);

        var items = found.Items.ToArray();
        for (var i = 1; i < items.Length; i++)
        {
            var current = items[i];
            var previous = items[i - 1];

            Assert.That(current.Name, Is.LessThanOrEqualTo(previous.Name));
        }

        return;

        Int32 IdentifierFactory() => entities.Count + 1;
    }

    private static String GetRandomName(Int32 length = 5)
    {
        var name = new StringBuilder();
        for (var i = 0; i <= length; i++)
            name.Append((Char) new Random().Next('A', 'Z'));

        return name.ToString();
    }

    [Test]
    public async Task Search_Async_ReturnsResults()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();

        creator.Create(new SurLaTable { Identifier = 1, Name = "ABC" }, scope);

        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Descending) }.OrderBy(_ => 1);
        var found = await searcher.Search(parameters, page, sort, scope, CancellationToken.None);

        Assert.That(found.Items, Has.Count.EqualTo(1));
    }

    [Test]
    public void Search_GreaterThanFilter_ReturnsMatchingEntities()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities, new CreationOptions<Int32>(IdentifierFactory));
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        for (var i = 1; i <= 10; i++)
            creator.Create(new SurLaTable { Identifier = i, Name = $"Item{i}" }, scope);

        var parameters = new[] { new FilterCriteria("Identifier", FilterOperation.GreaterThan, 5) };
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Ascending) }.OrderBy(_ => 1);
        var found = searcher.Search(parameters, page, sort, scope);

        Assert.That(found.TotalCount, Is.EqualTo(5));

        return;

        Int32 IdentifierFactory() => entities.Count + 1;
    }

    [Test]
    public void Search_GreaterThanOrEqualFilter_ReturnsMatchingEntities()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities, new CreationOptions<Int32>(IdentifierFactory));
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        for (var i = 1; i <= 10; i++)
            creator.Create(new SurLaTable { Identifier = i, Name = $"Item{i}" }, scope);

        var parameters = new[] { new FilterCriteria("Identifier", FilterOperation.GreaterThanOrEqual, 5) };
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Ascending) }.OrderBy(_ => 1);
        var found = searcher.Search(parameters, page, sort, scope);

        Assert.That(found.TotalCount, Is.EqualTo(6));

        return;

        Int32 IdentifierFactory() => entities.Count + 1;
    }

    [Test]
    public void Search_LessThanFilter_ReturnsMatchingEntities()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities, new CreationOptions<Int32>(IdentifierFactory));
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        for (var i = 1; i <= 10; i++)
            creator.Create(new SurLaTable { Identifier = i, Name = $"Item{i}" }, scope);

        var parameters = new[] { new FilterCriteria("Identifier", FilterOperation.LessThan, 5) };
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Ascending) }.OrderBy(_ => 1);
        var found = searcher.Search(parameters, page, sort, scope);

        Assert.That(found.TotalCount, Is.EqualTo(4));

        return;

        Int32 IdentifierFactory() => entities.Count + 1;
    }

    [Test]
    public void Search_NotEqualFilter_ReturnsMatchingEntities()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();

        creator.Create(new SurLaTable { Identifier = 1, Name = "ABC" }, scope);
        creator.Create(new SurLaTable { Identifier = 2, Name = "DEF" }, scope);

        var parameters = new[] { new FilterCriteria("Name", FilterOperation.NotEqual, "ABC") };
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Ascending) }.OrderBy(_ => 1);
        var found = searcher.Search(parameters, page, sort, scope);

        Assert.That(found.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public void Search_UnsupportedFilterOperation_ThrowsNotSupportedException()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();

        creator.Create(new SurLaTable { Identifier = 1, Name = "ABC" }, scope);

        var parameters = new[] { new FilterCriteria("Name", (FilterOperation) 99, "ABC") };
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Ascending) }.OrderBy(_ => 1);

        Assert.Throws<NotSupportedException>(Act);

        return;

        void Act() => searcher.Search(parameters, page, sort, scope);
    }

    public record SurLaTable : DomainEntityBase<Int32>, ICreationMoment
    {
        public DateTimeOffset CreationMoment { get; set; }

        public String Name { get; set; } = String.Empty;
    }
}
