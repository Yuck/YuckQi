using System;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using YuckQi.Data.Filtering;
using YuckQi.Data.Sql.EntityFramework.Handlers.Read;
using YuckQi.Data.Sorting;
using YuckQi.Domain.ValueObjects;
using YuckQi.Domain.ValueObjects.Abstract.Interfaces;

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
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(t => 1);

        Assert.Throws<ArgumentNullException>(() => handler.Search(parameters, page, sort, null));
    }

    [Test]
    public void SearchAsync_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new SearchHandler<SurLaTableRecord, Int32, TestDbContext>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(t => 1);

        Assert.ThrowsAsync<ArgumentNullException>(() => handler.Search(parameters, page, sort, null, CancellationToken.None));
    }

    [Test]
    public void Search_WithEmptyScope_ReturnsEmptyPage()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("Search_Empty").Options;
        using var context = new TestDbContext(options);
        var handler = new SearchHandler<SurLaTableRecord, Int32, TestDbContext>();
        var parameters = Array.Empty<FilterCriteria>();
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(t => 1);

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
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(t => 1);

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
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(t => 1);

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
        var sort = new[] { new SortCriteria("Id", SortOrder.Ascending) }.OrderBy(t => 1);

        context.SurLaTable.Add(new SurLaTableRecord { Id = 1, Name = "Match" });
        context.SurLaTable.Add(new SurLaTableRecord { Id = 2, Name = "Other" });
        context.SaveChanges();

        var result = handler.Search(parameters, page, sort, context);

        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items.First().Name, Is.EqualTo("Match"));
    }
}
