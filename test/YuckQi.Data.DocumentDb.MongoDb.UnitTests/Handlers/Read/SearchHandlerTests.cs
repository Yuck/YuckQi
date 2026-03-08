using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using YuckQi.Data.DocumentDb.MongoDb.Attributes;
using YuckQi.Data.DocumentDb.MongoDb.Handlers.Read;
using YuckQi.Data.Filtering;
using YuckQi.Data.Sorting;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Domain.ValueObjects;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.DocumentDb.MongoDb.UnitTests.Handlers.Read;

public class SearchHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new SearchHandler<SurLaTableDocument, Int32, IClientSessionHandle>();

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void MapperConstructor_CreatesInstance()
    {
        var handler = new SearchHandler<SurLaTableDocument, Int32, IClientSessionHandle, SurLaTableDocument>(new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Search_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new SearchHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(t => t.Expression);

        Assert.Throws<ArgumentNullException>(() => handler.Search(parameters, page, sort, null));
    }

    [Test]
    public void Search_WithNullScope_Async_ThrowsArgumentNullException()
    {
        var handler = new SearchHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(t => t.Expression);

        Assert.ThrowsAsync<ArgumentNullException>(() => handler.Search(parameters, page, sort, null, CancellationToken.None));
    }

    [Test]
    public void Search_WithScope_ReturnsPage()
    {
        var documents = new List<SurLaTableDocument> { new() { Identifier = 1, Name = "ABC" } };
        var scope = CreateMockScope(documents, 1);
        var handler = new SearchHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(t => t.Expression);

        var result = handler.Search(parameters, page, sort, scope.Object);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.PageNumber, Is.EqualTo(1));
            Assert.That(result.PageSize, Is.EqualTo(10));
        }
    }

    [Test]
    public async Task Search_Async_ReturnsPage()
    {
        var documents = new List<SurLaTableDocument> { new() { Identifier = 1, Name = "ABC" } };
        var scope = CreateMockScope(documents, 1, async: true);
        var handler = new SearchHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending) }.OrderBy(t => t.Expression);

        var result = await handler.Search(parameters, page, sort, scope.Object, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.PageNumber, Is.EqualTo(1));
        }
    }

    [Test]
    public void Search_WithDescendingSort_ReturnsPage()
    {
        var documents = new List<SurLaTableDocument> { new() { Identifier = 1, Name = "ABC" } };
        var scope = CreateMockScope(documents, 1);
        var handler = new SearchHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Descending) }.OrderBy(t => t.Expression);

        var result = handler.Search(parameters, page, sort, scope.Object);

        Assert.That(result, Is.Not.Null);
    }

    private static Mock<IClientSessionHandle> CreateMockScope(List<SurLaTableDocument> documents, Int64 count, Boolean async = false)
    {
        var findFluent = new Mock<IFindFluent<SurLaTableDocument, SurLaTableDocument>>();

        findFluent.Setup(t => t.Sort(It.IsAny<SortDefinition<SurLaTableDocument>>())).Returns(findFluent.Object);
        findFluent.Setup(t => t.Skip(It.IsAny<Int32?>())).Returns(findFluent.Object);
        findFluent.Setup(t => t.Limit(It.IsAny<Int32?>())).Returns(findFluent.Object);

        var cursor = new Mock<IAsyncCursor<SurLaTableDocument>>();

        cursor.SetupSequence(t => t.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
        cursor.SetupSequence(t => t.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);
        cursor.Setup(t => t.Current).Returns(documents);
        findFluent.Setup(t => t.ToCursor(It.IsAny<CancellationToken>())).Returns(cursor.Object);
        findFluent.Setup(t => t.ToCursorAsync(It.IsAny<CancellationToken>())).ReturnsAsync(cursor.Object);

        var collection = new Mock<IMongoCollection<SurLaTableDocument>>();

        collection.Setup(t => t.FindSync(It.IsAny<FilterDefinition<SurLaTableDocument>>(), It.IsAny<FindOptions<SurLaTableDocument, SurLaTableDocument>>(), It.IsAny<CancellationToken>())).Returns(cursor.Object);
        collection.Setup(t => t.FindAsync(It.IsAny<FilterDefinition<SurLaTableDocument>>(), It.IsAny<FindOptions<SurLaTableDocument, SurLaTableDocument>>(), It.IsAny<CancellationToken>())).ReturnsAsync(cursor.Object);
        collection.Setup(t => t.CountDocuments(It.IsAny<FilterDefinition<SurLaTableDocument>>(), It.IsAny<CountOptions>(), It.IsAny<CancellationToken>())).Returns(count);
        collection.Setup(t => t.CountDocumentsAsync(It.IsAny<FilterDefinition<SurLaTableDocument>>(), It.IsAny<CountOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(count);

        var database = new Mock<IMongoDatabase>();
        var client = new Mock<IMongoClient>();
        var scope = new Mock<IClientSessionHandle>();

        database.Setup(t => t.GetCollection<SurLaTableDocument>(It.IsAny<String>(), It.IsAny<MongoCollectionSettings>())).Returns(collection.Object);
        client.Setup(t => t.GetDatabase(It.IsAny<String>(), It.IsAny<MongoDatabaseSettings>())).Returns(database.Object);
        scope.Setup(t => t.Client).Returns(client.Object);

        return scope;
    }

    [Database("TestDb")]
    [Collection("TestCollection")]
    public class SurLaTableDocument : IDomainEntity<Int32>
    {
        [BsonId] public Int32 Identifier { get; set; }

        public String Name { get; set; } = String.Empty;
    }
}
