using System.Data;
using Dapper;
using Moq;
using NUnit.Framework;
using YuckQi.Data.Filtering;
using YuckQi.Data.Sorting;
using YuckQi.Data.Sql.Dapper.Abstract.Interfaces;
using YuckQi.Data.Sql.Dapper.Handlers.Read.Abstract;
using YuckQi.Domain.Entities.Abstract;
using YuckQi.Domain.ValueObjects;
using YuckQi.Domain.ValueObjects.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.Sql.Dapper.UnitTests.Handlers.Read;

public class SearchHandlerBaseTests
{
    [Test]
    public void Constructor_CreatesInstance()
    {
        var handler = new TestSearchHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Constructor_WithMapper_CreatesInstance()
    {
        var handler = new TestSearchHandlerWithMapper(CreateMockSqlGenerator(), new Dictionary<Type, DbType>(), new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Search_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new TestSearchHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());
        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(_ => 1);

        Assert.Throws<ArgumentNullException>(() => handler.Search(parameters, page, sort, null));
    }

    [Test]
    public void SearchAsync_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new TestSearchHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());
        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(_ => 1);

        Assert.ThrowsAsync<ArgumentNullException>(() => handler.Search(parameters, page, sort, null, CancellationToken.None));
    }

    [Test]
    public void Search_WithMockScope_ReturnsEmptyPage()
    {
        var handler = new TestSearchHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());
        var transaction = CreateMockTransaction();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(_ => 1);

        var result = handler.Search(parameters, page, sort, transaction);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Items, Is.Empty);
            Assert.That(result.TotalCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task SearchAsync_WithMockScope_ReturnsEmptyPage()
    {
        var handler = new TestSearchHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());
        var transaction = CreateMockTransaction();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(_ => 1);

        var result = await handler.Search(parameters, page, sort, transaction, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Items, Is.Empty);
            Assert.That(result.TotalCount, Is.EqualTo(0));
        });
    }

    private static ISqlGenerator CreateMockSqlGenerator()
    {
        var mock = new Mock<ISqlGenerator>();

        mock.Setup(t => t.GenerateCountQuery(It.IsAny<IReadOnlyCollection<FilterCriteria>>())).Returns("SELECT COUNT(*) FROM SurLaTable");
        mock.Setup(t => t.GenerateGetQuery(It.IsAny<IReadOnlyCollection<FilterCriteria>?>())).Returns("SELECT * FROM SurLaTable WHERE 1=1");
        mock.Setup(t => t.GenerateSearchQuery(It.IsAny<IReadOnlyCollection<FilterCriteria>>(), It.IsAny<IPage>(), It.IsAny<IOrderedEnumerable<SortCriteria>>())).Returns("SELECT * FROM SurLaTable WHERE 1=1");

        return mock.Object;
    }

    private static IDbTransaction CreateMockTransaction()
    {
        return MockDbFactory.CreateTransaction();
    }

    private class TestSearchHandler(ISqlGenerator sqlGenerator, IReadOnlyDictionary<Type, DbType> dbTypeMap) : SearchHandlerBase<SurLaTableEntity, Int32, IDbTransaction>(sqlGenerator, dbTypeMap);

    private class TestSearchHandlerWithMapper(ISqlGenerator sqlGenerator, IReadOnlyDictionary<Type, DbType> dbTypeMap, IMapper mapper) : SearchHandlerBase<SurLaTableEntity, Int32, IDbTransaction, SurLaTableEntity>(sqlGenerator, dbTypeMap, mapper);

    [Table("SurLaTable")]
    public record SurLaTableEntity : DomainEntityBase<Int32>
    {
        [Key]
        public Int32 Id { get => Identifier; set => Identifier = value; }

        public String Name { get; set; } = String.Empty;
    }
}
