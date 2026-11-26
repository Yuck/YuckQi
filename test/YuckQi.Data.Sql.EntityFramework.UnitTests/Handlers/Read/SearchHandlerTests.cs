using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using YuckQi.Data.Filtering;
using YuckQi.Data.Sorting;
using YuckQi.Data.Sql.EntityFramework.Handlers.Read;
using YuckQi.Domain.ValueObjects;

namespace YuckQi.Data.Sql.EntityFramework.UnitTests.Handlers.Read;

public class SearchHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new SearchHandler<SurLaTableRecord, Int32, TestDbContext>();

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Search_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new SearchHandler<SurLaTableRecord, Int32, TestDbContext>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) };

        Assert.Throws<ArgumentNullException>(() => handler.Search(parameters, page, sort, null));
    }

    [Test]
    public async Task SearchAsync_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new SearchHandler<SurLaTableRecord, Int32, TestDbContext>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) };

        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Search(parameters, page, sort, null, CancellationToken.None));
    }

    [Test]
    public void Search_WithEmptyScope_ReturnsEmptyPage()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("Search_Empty").Options;
        using var context = new TestDbContext(options);
        var handler = new SearchHandler<SurLaTableRecord, Int32, TestDbContext>();
        var parameters = Array.Empty<FilterCriteria>();
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) };

        var result = handler.Search(parameters, page, sort, context);

        Assert.That(result.TotalCount, Is.EqualTo(0));
        Assert.That(result.Items, Is.Empty);
    }

    [Test]
    public async Task SearchAsync_WithEmptyScope_ReturnsEmptyPage()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("Search_EmptyAsync").Options;
        await using var context = new TestDbContext(options);
        var handler = new SearchHandler<SurLaTableRecord, Int32, TestDbContext>();
        var parameters = Array.Empty<FilterCriteria>();
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) };

        var result = await handler.Search(parameters, page, sort, context, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(0));
        Assert.That(result.Items, Is.Empty);
    }

    [Test]
    public void Search_WithData_ReturnsPagedResults()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("Search_WithData").Options;
        using var context = new TestDbContext(options);
        var handler = new SearchHandler<SurLaTableRecord, Int32, TestDbContext>();
        var parameters = Array.Empty<FilterCriteria>();
        var page = new Page(1, 2);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) };

        context.SurLaTable.Add(new SurLaTableRecord { Id = 1, Name = "A" });
        context.SurLaTable.Add(new SurLaTableRecord { Id = 2, Name = "B" });
        context.SurLaTable.Add(new SurLaTableRecord { Id = 3, Name = "C" });
        context.SaveChanges();

        var result = handler.Search(parameters, page, sort, context);

        Assert.That(result.TotalCount, Is.EqualTo(3));
        Assert.That(result.Items.Count, Is.EqualTo(2));
    }

    [Test]
    public void Search_WithFilterCriteria_ReturnsMatchingResults()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("Search_Filter").Options;
        using var context = new TestDbContext(options);
        var handler = new SearchHandler<SurLaTableRecord, Int32, TestDbContext>();
        var parameters = new[] { new FilterCriteria("Name", "Match") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Id", SortOrder.Ascending) };

        context.SurLaTable.Add(new SurLaTableRecord { Id = 1, Name = "Match" });
        context.SurLaTable.Add(new SurLaTableRecord { Id = 2, Name = "Other" });
        context.SaveChanges();

        var result = handler.Search(parameters, page, sort, context);

        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items.First().Name, Is.EqualTo("Match"));
    }

    [Test]
    public void Search_WithContainsFilter_ReturnsMatchingResults()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("Search_Contains").Options;
        using var context = new TestDbContext(options);
        var handler = new SearchHandler<SurLaTableRecord, Int32, TestDbContext>();
        var parameters = new[] { new FilterCriteria("Name", FilterOperation.Contains, "Guy") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Id", SortOrder.Ascending) };

        context.SurLaTable.Add(new SurLaTableRecord { Id = 1, Name = "Some Guy" });
        context.SurLaTable.Add(new SurLaTableRecord { Id = 2, Name = "Someone Else" });
        context.SaveChanges();

        var result = handler.Search(parameters, page, sort, context);

        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items.First().Name, Is.EqualTo("Some Guy"));
    }

    [Test]
    public void Search_WithMultipleSortCriteria_HonoursCallerPriority()
    {
        using var context = CreatePeopleContext("Search_MultipleSort");
        var handler = new SearchHandler<PersonRecord, Int32, TestDbContext>();
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("LastName", SortOrder.Descending), new SortCriteria("FirstName", SortOrder.Ascending) };

        var result = handler.Search(Array.Empty<FilterCriteria>(), page, sort, context);

        Assert.That(result.Items.Select(t => t.Id), Is.EqualTo(new[] { 1, 3, 4, 2 }));
    }

    [Test]
    public async Task Search_Async_WithMultipleSortCriteria_PagesInCallerPriority()
    {
        using var context = CreatePeopleContext("Search_MultipleSort_Paged");
        var handler = new SearchHandler<PersonRecord, Int32, TestDbContext>();
        var page = new Page(2, 2);
        var sort = new[] { new SortCriteria("LastName", SortOrder.Descending), new SortCriteria("FirstName", SortOrder.Ascending) };

        var result = await handler.Search(Array.Empty<FilterCriteria>(), page, sort, context, CancellationToken.None);

        Assert.That(result.Items.Select(t => t.Id), Is.EqualTo(new[] { 4, 2 }));
    }

    [Test]
    public void Search_WithNestedSortCriteria_OrdersByNestedProperty()
    {
        using var context = CreatePeopleContext("Search_NestedSort");
        var handler = new SearchHandler<PersonRecord, Int32, TestDbContext>();
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Address.City", SortOrder.Ascending), new SortCriteria("Id", SortOrder.Descending) };

        var result = handler.Search(Array.Empty<FilterCriteria>(), page, sort, context);

        Assert.That(result.Items.Select(t => t.Id), Is.EqualTo(new[] { 4, 2, 3, 1 }));
    }

    [Test]
    public void Search_WithNoSortCriteria_OrdersByPrimaryKey()
    {
        var recorder = new QueryExpressionRecorder();
        using var context = CreatePeopleContext("Search_NoSort", recorder);
        var handler = new SearchHandler<PersonRecord, Int32, TestDbContext>();
        var page = new Page(1, 10);

        var result = handler.Search(Array.Empty<FilterCriteria>(), page, Array.Empty<SortCriteria>(), context);
        var search = recorder.Expressions.Select(t => t.ToString()).Single(t => t.Contains(".Skip("));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Items.Select(t => t.Id), Is.EqualTo(new[] { 1, 2, 3, 4 }));
            Assert.That(search, Does.Contain(".OrderBy(").And.Contain("\"Id\""));
        }
    }

    [Test]
    public async Task Search_Async_WithNoSortCriteria_PagesByPrimaryKey()
    {
        var recorder = new QueryExpressionRecorder();
        using var context = CreatePeopleContext("Search_NoSort_Paged", recorder);
        var handler = new SearchHandler<PersonRecord, Int32, TestDbContext>();
        var page = new Page(2, 2);

        var result = await handler.Search(Array.Empty<FilterCriteria>(), page, Array.Empty<SortCriteria>(), context, CancellationToken.None);
        var search = recorder.Expressions.Select(t => t.ToString()).Single(t => t.Contains(".Skip("));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Items.Select(t => t.Id), Is.EqualTo(new[] { 3, 4 }));
            Assert.That(search, Does.Contain(".OrderBy(").And.Contain("\"Id\""));
        }
    }

    [Test]
    public void Search_WithSortCriteria_DoesNotAddKeyOrdering()
    {
        var recorder = new QueryExpressionRecorder();
        using var context = CreatePeopleContext("Search_Sort_NoKey", recorder);
        var handler = new SearchHandler<PersonRecord, Int32, TestDbContext>();
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("LastName", SortOrder.Ascending) };

        handler.Search(Array.Empty<FilterCriteria>(), page, sort, context);
        var search = recorder.Expressions.Select(t => t.ToString()).Single(t => t.Contains(".Skip("));

        Assert.That(search, Does.Not.Contain("\"Id\""));
    }

    private static TestDbContext CreatePeopleContext(String name, QueryExpressionRecorder? recorder = null)
    {
        var builder = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(name);
        if (recorder is not null)
            builder.AddInterceptors(recorder);

        var options = builder.Options;
        var context = new TestDbContext(options);

        context.People.Add(new PersonRecord { Id = 1, FirstName = "Cal", LastName = "Smith", Address = new PersonAddress { City = "Oslo" } });
        context.People.Add(new PersonRecord { Id = 2, FirstName = "Bob", LastName = "Jones", Address = new PersonAddress { City = "Bergen" } });
        context.People.Add(new PersonRecord { Id = 3, FirstName = "Dan", LastName = "Smith", Address = new PersonAddress { City = "Oslo" } });
        context.People.Add(new PersonRecord { Id = 4, FirstName = "Abe", LastName = "Jones", Address = new PersonAddress { City = "Bergen" } });
        context.SaveChanges();
        context.ChangeTracker.Clear();

        return context;
    }
}
