using System.Data;
using Dapper;
using Moq;
using NUnit.Framework;
using YuckQi.Data.Filtering;
using YuckQi.Data.Sorting;
using YuckQi.Data.Sql.Dapper.Abstract.Interfaces;
using YuckQi.Data.Sql.Dapper.Handlers.Read.Abstract;
using YuckQi.Domain.Entities.Abstract;
using YuckQi.Domain.ValueObjects.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.Sql.Dapper.UnitTests.Handlers.Read;

public class RetrievalHandlerBaseTests
{
    [Test]
    public void Constructor_CreatesInstance()
    {
        var handler = new TestRetrievalHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Constructor_WithMapper_CreatesInstance()
    {
        var handler = new TestRetrievalHandlerWithMapper(CreateMockSqlGenerator(), new Dictionary<Type, DbType>(), new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Get_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new TestRetrievalHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());

        Assert.Throws<ArgumentNullException>(() => handler.Get(1, null));
    }

    [Test]
    public void GetAsync_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new TestRetrievalHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());

        Assert.ThrowsAsync<ArgumentNullException>(() => handler.Get(1, null, CancellationToken.None));
    }

    [Test]
    public void Get_ByIdentifier_WithMockScope_ReturnsNull()
    {
        var handler = new TestRetrievalHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());
        var transaction = CreateMockTransaction();

        var result = handler.Get(1, transaction);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAsync_ByIdentifier_WithMockScope_ReturnsNull()
    {
        var handler = new TestRetrievalHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());
        var transaction = CreateMockTransaction();

        var result = await handler.Get(1, transaction, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Get_ByFilterCriteria_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new TestRetrievalHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        Assert.Throws<ArgumentNullException>(() => handler.Get(parameters, null));
    }

    [Test]
    public void GetAsync_ByFilterCriteria_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new TestRetrievalHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        Assert.ThrowsAsync<ArgumentNullException>(() => handler.Get(parameters, null, CancellationToken.None));
    }

    [Test]
    public void Get_ByFilterCriteria_WithMockScope_ReturnsNull()
    {
        var handler = new TestRetrievalHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());
        var transaction = CreateMockTransaction();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        var result = handler.Get(parameters, transaction);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAsync_ByFilterCriteria_WithMockScope_ReturnsNull()
    {
        var handler = new TestRetrievalHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());
        var transaction = CreateMockTransaction();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        var result = await handler.Get(parameters, transaction, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetList_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new TestRetrievalHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());

        Assert.Throws<ArgumentNullException>(() => handler.GetList(null));
    }

    [Test]
    public void GetListAsync_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new TestRetrievalHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());

        Assert.ThrowsAsync<ArgumentNullException>(() => handler.GetList(null, CancellationToken.None));
    }

    [Test]
    public void GetList_WithMockScope_ReturnsEmptyCollection()
    {
        var handler = new TestRetrievalHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());
        var transaction = CreateMockTransaction();

        var result = handler.GetList(transaction);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetListAsync_WithMockScope_ReturnsEmptyCollection()
    {
        var handler = new TestRetrievalHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());
        var transaction = CreateMockTransaction();

        var result = await handler.GetList(transaction, CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetList_ByFilterCriteria_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new TestRetrievalHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        Assert.Throws<ArgumentNullException>(() => handler.GetList(parameters, null));
    }

    [Test]
    public void GetListAsync_ByFilterCriteria_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new TestRetrievalHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        Assert.ThrowsAsync<ArgumentNullException>(() => handler.GetList(parameters, null, CancellationToken.None));
    }

    [Test]
    public void GetList_ByFilterCriteria_WithMockScope_ReturnsEmptyCollection()
    {
        var handler = new TestRetrievalHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());
        var transaction = CreateMockTransaction();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        var result = handler.GetList(parameters, transaction);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetListAsync_ByFilterCriteria_WithMockScope_ReturnsEmptyCollection()
    {
        var handler = new TestRetrievalHandler(CreateMockSqlGenerator(), new Dictionary<Type, DbType>());
        var transaction = CreateMockTransaction();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        var result = await handler.GetList(parameters, transaction, CancellationToken.None);

        Assert.That(result, Is.Empty);
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

    private class TestRetrievalHandler(ISqlGenerator sqlGenerator, IReadOnlyDictionary<Type, DbType> dbTypeMap) : RetrievalHandlerBase<SurLaTableEntity, Int32, IDbTransaction>(sqlGenerator, dbTypeMap);

    private class TestRetrievalHandlerWithMapper(ISqlGenerator sqlGenerator, IReadOnlyDictionary<Type, DbType> dbTypeMap, IMapper mapper) : RetrievalHandlerBase<SurLaTableEntity, Int32, IDbTransaction, SurLaTableEntity>(sqlGenerator, dbTypeMap, mapper);

    [Table("SurLaTable")]
    public record SurLaTableEntity : DomainEntityBase<Int32>
    {
        [Key]
        public Int32 Id { get => Identifier; set => Identifier = value; }

        public String Name { get; set; } = String.Empty;
    }
}
