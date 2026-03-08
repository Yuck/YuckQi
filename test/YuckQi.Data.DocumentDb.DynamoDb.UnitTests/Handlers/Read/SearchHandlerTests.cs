using Amazon.DynamoDBv2.DataModel;
using Moq;
using NUnit.Framework;
using YuckQi.Data.DocumentDb.DynamoDb.Handlers.Read;
using YuckQi.Data.Filtering;
using YuckQi.Data.Sorting;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Domain.ValueObjects;
using YuckQi.Domain.ValueObjects.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.DocumentDb.DynamoDb.UnitTests.Handlers.Read;

public class SearchHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new SearchHandler<SurLaTableDocument, Int32, IDynamoDBContext>();

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void MapperConstructor_CreatesInstance()
    {
        var handler = new SearchHandler<SurLaTableDocument, Int32, IDynamoDBContext, SurLaTableDocument>(new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Search_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new SearchHandler<SurLaTableDocument, Int32, IDynamoDBContext>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(t => t.Expression);

        Assert.Throws<ArgumentNullException>(Act);

        return;

        void Act() => handler.Search(parameters, page, sort, null);
    }

    [Test]
    public void Search_Async_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new SearchHandler<SurLaTableDocument, Int32, IDynamoDBContext>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(t => t.Expression);

        Assert.ThrowsAsync<ArgumentNullException>(Act);

        return;

        Task Act() => handler.Search(parameters, page, sort, null, CancellationToken.None);
    }

    [Test]
    public void Search_Async_WithScope_ThrowsNotImplementedException()
    {
        var scope = new Mock<IDynamoDBContext>();
        var handler = new SearchHandler<SurLaTableDocument, Int32, IDynamoDBContext>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(t => t.Expression);

        Assert.ThrowsAsync<NotImplementedException>(Act);

        return;

        Task Act() => handler.Search(parameters, page, sort, scope.Object, CancellationToken.None);
    }

    [Test]
    public void Search_WithObjectParameters_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new SearchHandler<SurLaTableDocument, Int32, IDynamoDBContext>();
        var parameters = new { Name = "ABC" };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(t => t.Expression);

        Assert.Throws<ArgumentNullException>(Act);

        return;

        void Act() => handler.Search(parameters, page, sort, null);
    }

    [Test]
    public void Search_Async_WithObjectParameters_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new SearchHandler<SurLaTableDocument, Int32, IDynamoDBContext>();
        var parameters = new { Name = "ABC" };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(t => t.Expression);

        Assert.ThrowsAsync<ArgumentNullException>(Act);

        return;

        Task Act() => handler.Search(parameters, page, sort, null, CancellationToken.None);
    }

    [Test]
    public void Search_WithScope_WhenDoSearchReturnsEmpty_InvokesDoCount()
    {
        var scope = new Mock<IDynamoDBContext>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(t => t.Expression);
        var handler = new SearchHandlerThatReturnsEmptyForSearch<SurLaTableDocument, Int32, IDynamoDBContext>();

        Assert.Throws<NullReferenceException>(Act);

        return;

        void Act() => handler.Search(parameters, page, sort, scope.Object);
    }

    [Test]
    public void Search_Async_WithScope_WhenDoSearchReturnsEmpty_InvokesDoCount()
    {
        var scope = new Mock<IDynamoDBContext>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(t => t.Expression);
        var handler = new SearchHandlerThatReturnsEmptyForSearch<SurLaTableDocument, Int32, IDynamoDBContext>();

        Assert.ThrowsAsync<NullReferenceException>(Act);

        return;

        Task Act() => handler.Search(parameters, page, sort, scope.Object, CancellationToken.None);
    }

    private sealed class SearchHandlerThatReturnsEmptyForSearch<TDomainEntity, TIdentifier, TScope> : SearchHandler<TDomainEntity, TIdentifier, TScope> where TDomainEntity : IDomainEntity<TIdentifier> where TIdentifier : IEquatable<TIdentifier> where TScope : IDynamoDBContext?
    {
        protected override IReadOnlyCollection<TDomainEntity> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope)
        {
            return Array.Empty<TDomainEntity>();
        }

        protected override Task<IReadOnlyCollection<TDomainEntity>> DoSearch(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort, TScope? scope, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<TDomainEntity>>(Array.Empty<TDomainEntity>());
        }
    }

    public class SurLaTableDocument : IDomainEntity<Int32>
    {
        public Int32 Identifier { get; set; }

        public String Name { get; set; } = String.Empty;
    }
}
