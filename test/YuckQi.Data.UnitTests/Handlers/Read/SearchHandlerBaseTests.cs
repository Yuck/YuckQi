using NUnit.Framework;
using YuckQi.Data.Filtering;
using YuckQi.Data.Handlers.Read.Abstract;
using YuckQi.Data.Sorting;
using YuckQi.Domain.Entities.Abstract;
using YuckQi.Domain.ValueObjects;
using YuckQi.Domain.ValueObjects.Abstract.Interfaces;

namespace YuckQi.Data.UnitTests.Handlers.Read;

public class SearchHandlerBaseTests
{
    [TestCase(0, 10)]
    [TestCase(-1, 10)]
    [TestCase(1, 0)]
    [TestCase(1, -1)]
    [TestCase(Int32.MaxValue, 10)]
    public void Search_WithInvalidPage_ThrowsBeforeSearching(Int32 pageNumber, Int32 pageSize)
    {
        var handler = new StubSearchHandler();
        var page = new Page(pageNumber, pageSize);

        Assert.Throws<ArgumentOutOfRangeException>(() => handler.Search(new[] { new FilterCriteria("Name", "ABC") }, page, CreateSort(), new Object()));
        Assert.That(handler.Searched, Is.False);
    }

    [TestCase(0, 10)]
    [TestCase(-1, 10)]
    [TestCase(1, 0)]
    [TestCase(1, -1)]
    [TestCase(Int32.MaxValue, 10)]
    public async Task Search_Async_WithInvalidPage_ThrowsBeforeSearching(Int32 pageNumber, Int32 pageSize)
    {
        var handler = new StubSearchHandler();
        var page = new Page(pageNumber, pageSize);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => handler.Search(new[] { new FilterCriteria("Name", "ABC") }, page, CreateSort(), new Object(), CancellationToken.None));
        Assert.That(handler.Searched, Is.False);
    }

    [Test]
    public void Search_ByObject_WithInvalidPage_ThrowsBeforeSearching()
    {
        var handler = new StubSearchHandler();
        var page = new Page(0, 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => handler.Search(new { Name = "ABC" }, page, CreateSort(), new Object()));
        Assert.That(handler.Searched, Is.False);
    }

    [Test]
    public void Search_ByExpression_WithInvalidPage_ThrowsBeforeSearching()
    {
        var handler = new StubSearchHandler();
        var page = new Page(1, 0);

        Assert.Throws<ArgumentOutOfRangeException>(() => handler.Search(t => t.Name == "ABC", page, CreateSort(), new Object()));
        Assert.That(handler.Searched, Is.False);
    }

    [Test]
    public void Search_WithNullPage_ThrowsArgumentNullException()
    {
        var handler = new StubSearchHandler();

        Assert.Throws<ArgumentNullException>(() => handler.Search(new[] { new FilterCriteria("Name", "ABC") }, null!, CreateSort(), new Object()));
        Assert.That(handler.Searched, Is.False);
    }

    [Test]
    public void Search_WithValidPage_ReturnsPage()
    {
        var handler = new StubSearchHandler();
        var page = new Page(2, 10);

        var result = handler.Search(new[] { new FilterCriteria("Name", "ABC") }, page, CreateSort(), new Object());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(handler.Searched, Is.True);
            Assert.That(result, Is.EqualTo(new Page<SurLaTable>([], 0, 2, 10)));
        }
    }

    private static IReadOnlyList<SortCriteria> CreateSort()
    {
        return new[] { new SortCriteria("Name", SortOrder.Ascending) };
    }

    public record SurLaTable : DomainEntityBase<Int32>
    {
        public String Name { get; init; } = String.Empty;
    }

    private class StubSearchHandler : SearchHandlerBase<SurLaTable, Int32, Object>
    {
        public Boolean Searched { get; private set; }

        protected override Int32 DoCount(IReadOnlyCollection<FilterCriteria> parameters, Object? scope)
        {
            return 0;
        }

        protected override Task<Int32> DoCount(IReadOnlyCollection<FilterCriteria> parameters, Object? scope, CancellationToken cancellationToken)
        {
            return Task.FromResult(DoCount(parameters, scope));
        }

        protected override IReadOnlyCollection<SurLaTable> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IReadOnlyList<SortCriteria> sort, Object? scope)
        {
            Searched = true;

            return [];
        }

        protected override Task<IReadOnlyCollection<SurLaTable>> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IReadOnlyList<SortCriteria> sort, Object? scope, CancellationToken cancellationToken)
        {
            return Task.FromResult(DoSearch(parameters, page, sort, scope));
        }
    }
}
