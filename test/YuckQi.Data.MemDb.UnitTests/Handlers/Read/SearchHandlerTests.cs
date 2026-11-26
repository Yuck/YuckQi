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
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Descending) };
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
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Descending) };
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
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Descending) };
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
            creator.Create(new SurLaTable { Name = "ABC" }, scope);

        var parameters = new[] { new FilterCriteria("Identifier", FilterOperation.LessThanOrEqual, 25) };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Descending) };
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
            creator.Create(new SurLaTable { Name = GetRandomName() }, scope);

        Assert.That(entities, Has.Count.EqualTo(50));

        var parameters = new[] { new FilterCriteria("Identifier", FilterOperation.LessThanOrEqual, 25) };
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Name", SortOrder.Descending) };
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
            creator.Create(new SurLaTable { Name = GetRandomName() }, scope);

        Assert.That(entities, Has.Count.EqualTo(50));

        var parameters = new[] { new FilterCriteria("Identifier", FilterOperation.In, new[] { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50 }) };
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Name", SortOrder.Descending) };
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
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Descending) };
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
            creator.Create(new SurLaTable { Name = $"Item{i}" }, scope);

        var parameters = new[] { new FilterCriteria("Identifier", FilterOperation.GreaterThan, 5) };
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Ascending) };
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
            creator.Create(new SurLaTable { Name = $"Item{i}" }, scope);

        var parameters = new[] { new FilterCriteria("Identifier", FilterOperation.GreaterThanOrEqual, 5) };
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Ascending) };
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
            creator.Create(new SurLaTable { Name = $"Item{i}" }, scope);

        var parameters = new[] { new FilterCriteria("Identifier", FilterOperation.LessThan, 5) };
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Ascending) };
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
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Ascending) };
        var found = searcher.Search(parameters, page, sort, scope);

        Assert.That(found.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public void Search_ContainsFilter_WhenMatch_ReturnsMatchingEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();

        creator.Create(new SurLaTable { Identifier = 1, Name = "Some Guy" }, scope);
        creator.Create(new SurLaTable { Identifier = 2, Name = "Someone Else" }, scope);

        var parameters = new[] { new FilterCriteria("Name", FilterOperation.Contains, "Guy") };
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Ascending) };
        var found = searcher.Search(parameters, page, sort, scope);

        Assert.Multiple(() =>
        {
            Assert.That(found.TotalCount, Is.EqualTo(1));
            Assert.That(found.Items.Single().Name, Is.EqualTo("Some Guy"));
        });
    }

    [Test]
    public void Search_ContainsFilter_WhenNoMatch_DoesNotContainEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();

        creator.Create(new SurLaTable { Identifier = 1, Name = "Some Guy" }, scope);

        var parameters = new[] { new FilterCriteria("Name", FilterOperation.Contains, "Nope") };
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Ascending) };
        var found = searcher.Search(parameters, page, sort, scope);

        Assert.That(found.TotalCount, Is.EqualTo(0));
    }

    [Test]
    public void Search_ContainsFilter_IsCaseInsensitive()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();

        creator.Create(new SurLaTable { Identifier = 1, Name = "Some Guy" }, scope);

        var parameters = new[] { new FilterCriteria("Name", FilterOperation.Contains, "guy") };
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Ascending) };
        var found = searcher.Search(parameters, page, sort, scope);

        Assert.That(found.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public void Search_ContainsFilter_TreatsWildcardsAsLiteral()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();

        creator.Create(new SurLaTable { Identifier = 1, Name = "100% off" }, scope);
        creator.Create(new SurLaTable { Identifier = 2, Name = "100X off" }, scope);

        var parameters = new[] { new FilterCriteria("Name", FilterOperation.Contains, "100%") };
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Ascending) };
        var found = searcher.Search(parameters, page, sort, scope);

        Assert.Multiple(() =>
        {
            Assert.That(found.TotalCount, Is.EqualTo(1));
            Assert.That(found.Items.Single().Name, Is.EqualTo("100% off"));
        });
    }

    [Test]
    public void Search_ContainsFilter_WithNullValue_ThrowsArgumentException()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();

        creator.Create(new SurLaTable { Identifier = 1, Name = "Some Guy" }, scope);

        var parameters = new[] { new FilterCriteria("Name", FilterOperation.Contains, null) };
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Ascending) };

        Assert.Throws<ArgumentException>(Act);

        return;

        void Act() => searcher.Search(parameters, page, sort, scope);
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
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Ascending) };

        Assert.Throws<NotSupportedException>(Act);

        return;

        void Act() => searcher.Search(parameters, page, sort, scope);
    }

    [Test]
    public void Search_UnknownFilterOperation_ThrowsArgumentException()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();

        creator.Create(new SurLaTable { Identifier = 1, Name = "ABC" }, scope);

        var parameters = new[] { new FilterCriteria("Name", FilterOperation.Unknown, "ABC") };
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Ascending) };

        Assert.Throws<ArgumentException>(Act);

        return;

        void Act() => searcher.Search(parameters, page, sort, scope);
    }

    [Test]
    public void Search_ByObject_WithEmptyDictionary_ThrowsArgumentException()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Ascending) };

        Assert.Throws<ArgumentException>(Act);

        return;

        void Act() => searcher.Search(new Dictionary<String, Object?>(), page, sort, new Object());
    }

    [Test]
    public void Search_ByObject_WithScalar_ThrowsArgumentException()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var searcher = new SearchHandler<SurLaTable, Int32, Object>(entities);
        var page = new Page(1, 50);
        var sort = new[] { new SortCriteria("Identifier", SortOrder.Ascending) };

        Assert.Throws<ArgumentException>(Act);

        return;

        void Act() => searcher.Search(1, page, sort, new Object());
    }

    [Test]
    public void Search_WithMultipleSortCriteria_HonoursCallerPriority()
    {
        var entities = CreatePeople();
        var searcher = new SearchHandler<Person, Int32, Object>(entities);
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("LastName", SortOrder.Descending), new SortCriteria("FirstName", SortOrder.Ascending) };

        var found = searcher.Search(Array.Empty<FilterCriteria>(), page, sort, new Object());

        Assert.That(found.Items.Select(t => t.Identifier), Is.EqualTo(new[] { 1, 3, 4, 2 }));
    }

    [Test]
    public void Search_WithMultipleSortCriteria_PagesInCallerPriority()
    {
        var entities = CreatePeople();
        var searcher = new SearchHandler<Person, Int32, Object>(entities);
        var page = new Page(2, 2);
        var sort = new[] { new SortCriteria("LastName", SortOrder.Descending), new SortCriteria("FirstName", SortOrder.Ascending) };

        var found = searcher.Search(Array.Empty<FilterCriteria>(), page, sort, new Object());

        Assert.That(found.Items.Select(t => t.Identifier), Is.EqualTo(new[] { 4, 2 }));
    }

    [Test]
    public async Task Search_Async_WithNestedSortCriteria_OrdersByNestedProperty()
    {
        var entities = CreatePeople();
        var searcher = new SearchHandler<Person, Int32, Object>(entities);
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Address.City", SortOrder.Ascending), new SortCriteria("Identifier", SortOrder.Descending) };

        var found = await searcher.Search(Array.Empty<FilterCriteria>(), page, sort, new Object(), CancellationToken.None);

        Assert.That(found.Items.Select(t => t.Identifier), Is.EqualTo(new[] { 4, 2, 3, 1 }));
    }

    [Test]
    public void Search_WithNestedSortCriteria_WhenIntermediateIsNull_SortsNullsFirst()
    {
        var entities = CreatePeople();
        var searcher = new SearchHandler<Person, Int32, Object>(entities);
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Address.City", SortOrder.Ascending), new SortCriteria("Identifier", SortOrder.Ascending) };

        entities[5] = new Person { Identifier = 5, FirstName = "Eve", LastName = "Young", Address = null };

        var found = searcher.Search(Array.Empty<FilterCriteria>(), page, sort, new Object());

        Assert.That(found.Items.Select(t => t.Identifier), Is.EqualTo(new[] { 5, 2, 4, 1, 3 }));
    }

    private static ConcurrentDictionary<Int32, Person> CreatePeople()
    {
        var people = new[]
        {
            new Person { Identifier = 1, FirstName = "Cal", LastName = "Smith", Address = new PersonAddress { City = "Oslo" } },
            new Person { Identifier = 2, FirstName = "Bob", LastName = "Jones", Address = new PersonAddress { City = "Bergen" } },
            new Person { Identifier = 3, FirstName = "Dan", LastName = "Smith", Address = new PersonAddress { City = "Oslo" } },
            new Person { Identifier = 4, FirstName = "Abe", LastName = "Jones", Address = new PersonAddress { City = "Bergen" } }
        };

        return new ConcurrentDictionary<Int32, Person>(people.Select(t => new KeyValuePair<Int32, Person>(t.Identifier, t)));
    }

    public record Person : DomainEntityBase<Int32>
    {
        public PersonAddress? Address { get; set; } = new();

        public String FirstName { get; set; } = String.Empty;

        public String LastName { get; set; } = String.Empty;
    }

    public record PersonAddress
    {
        public String City { get; set; } = String.Empty;
    }

    public record SurLaTable : DomainEntityBase<Int32>, ICreationMoment
    {
        public DateTimeOffset CreationMoment { get; set; }

        public String Name { get; set; } = String.Empty;
    }
}
