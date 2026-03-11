using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using YuckQi.Data.DocumentDb.MongoDb.Attributes;
using YuckQi.Data.DocumentDb.MongoDb.Handlers.Read;
using YuckQi.Data.Filtering;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.DocumentDb.MongoDb.UnitTests.Handlers.Read;

public class RetrievalHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IClientSessionHandle>();

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void MapperConstructor_CreatesInstance()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IClientSessionHandle, SurLaTableDocument>(new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Get_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IClientSessionHandle>();

        Assert.Throws<ArgumentNullException>(() => handler.Get(1, null));
    }

    [Test]
    public void Get_WithFilterCriteria_AndNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        Assert.Throws<ArgumentNullException>(() => handler.Get(parameters, null));
    }

    [Test]
    public void GetList_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IClientSessionHandle>();

        Assert.Throws<ArgumentNullException>(() => handler.GetList(null));
    }

    [Test]
    public void GetList_WithFilterCriteria_AndNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        Assert.Throws<ArgumentNullException>(() => handler.GetList(parameters, null));
    }

    [Test]
    public void Get_ByIdentifier_ReturnsEntity()
    {
        var document = new SurLaTableDocument { Identifier = 1, Name = "ABC" };
        var cursor = CreateMockCursor(document);
        var scope = CreateMockScope(cursor.Object);
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IClientSessionHandle>();

        var result = handler.Get(1, scope.Object);

        Assert.That(result?.Identifier, Is.EqualTo(1));
    }

    [Test]
    public async Task Get_ByIdentifier_Async_ReturnsEntity()
    {
        var document = new SurLaTableDocument { Identifier = 1, Name = "ABC" };
        var cursor = CreateMockCursor(document);
        var scope = CreateMockScope(asyncCursor: cursor.Object);
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IClientSessionHandle>();

        var result = await handler.Get(1, scope.Object, CancellationToken.None);

        Assert.That(result?.Identifier, Is.EqualTo(1));
    }

    [Test]
    public void Get_ByIdentifier_WhenNotFound_ReturnsNull()
    {
        var cursor = CreateEmptyMockCursor();
        var scope = CreateMockScope(cursor.Object);
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IClientSessionHandle>();

        var result = handler.Get(999, scope.Object);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Get_ByFilterCriteria_ReturnsEntity()
    {
        var document = new SurLaTableDocument { Identifier = 1, Name = "ABC" };
        var cursor = CreateMockCursor(document);
        var scope = CreateMockScope(cursor.Object);
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        var result = handler.Get(parameters, scope.Object);

        Assert.That(result?.Identifier, Is.EqualTo(1));
    }

    [Test]
    public async Task Get_ByFilterCriteria_Async_ReturnsEntity()
    {
        var document = new SurLaTableDocument { Identifier = 1, Name = "ABC" };
        var cursor = CreateMockCursor(document);
        var scope = CreateMockScope(asyncCursor: cursor.Object);
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        var result = await handler.Get(parameters, scope.Object, CancellationToken.None);

        Assert.That(result?.Identifier, Is.EqualTo(1));
    }

    [Test]
    public void GetList_ReturnsEntities()
    {
        var documents = new[] { new SurLaTableDocument { Identifier = 1, Name = "ABC" }, new SurLaTableDocument { Identifier = 2, Name = "DEF" } };
        var cursor = CreateMockCursorForList(documents);
        var scope = CreateMockScope(cursor.Object);
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IClientSessionHandle>();

        var result = handler.GetList(scope.Object);

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetList_Async_ReturnsEntities()
    {
        var documents = new[] { new SurLaTableDocument { Identifier = 1, Name = "ABC" }, new SurLaTableDocument { Identifier = 2, Name = "DEF" } };
        var cursor = CreateMockCursorForList(documents);
        var scope = CreateMockScope(asyncCursor: cursor.Object);
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IClientSessionHandle>();

        var result = await handler.GetList(scope.Object, CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public void GetList_WithFilterCriteria_ReturnsEntities()
    {
        var documents = new[] { new SurLaTableDocument { Identifier = 1, Name = "ABC" } };
        var cursor = CreateMockCursorForList(documents);
        var scope = CreateMockScope(cursor.Object);
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        var result = handler.GetList(parameters, scope.Object);

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetList_WithFilterCriteria_Async_ReturnsEntities()
    {
        var documents = new[] { new SurLaTableDocument { Identifier = 1, Name = "ABC" } };
        var cursor = CreateMockCursorForList(documents);
        var scope = CreateMockScope(asyncCursor: cursor.Object);
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        var result = await handler.GetList(parameters, scope.Object, CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
    }

    private static Mock<IAsyncCursor<SurLaTableDocument>> CreateMockCursor(SurLaTableDocument document)
    {
        var cursor = new Mock<IAsyncCursor<SurLaTableDocument>>();

        cursor.SetupSequence(t => t.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
        cursor.SetupSequence(t => t.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);
        cursor.Setup(t => t.Current).Returns([document]);

        return cursor;
    }

    private static Mock<IAsyncCursor<SurLaTableDocument>> CreateEmptyMockCursor()
    {
        var cursor = new Mock<IAsyncCursor<SurLaTableDocument>>();

        cursor.Setup(t => t.MoveNext(It.IsAny<CancellationToken>())).Returns(false);
        cursor.Setup(t => t.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        return cursor;
    }

    private static Mock<IAsyncCursor<SurLaTableDocument>> CreateMockCursorForList(SurLaTableDocument[] documents)
    {
        var cursor = new Mock<IAsyncCursor<SurLaTableDocument>>();

        cursor.SetupSequence(t => t.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
        cursor.SetupSequence(t => t.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);
        cursor.Setup(t => t.Current).Returns(documents);

        return cursor;
    }

    private static Mock<IClientSessionHandle> CreateMockScope(IAsyncCursor<SurLaTableDocument>? syncCursor = null, IAsyncCursor<SurLaTableDocument>? asyncCursor = null)
    {
        var collection = new Mock<IMongoCollection<SurLaTableDocument>>();

        if (syncCursor is not null)
            collection.Setup(t => t.FindSync(It.IsAny<FilterDefinition<SurLaTableDocument>>(), It.IsAny<FindOptions<SurLaTableDocument, SurLaTableDocument>>(), It.IsAny<CancellationToken>())).Returns(syncCursor);

        if (asyncCursor is not null)
            collection.Setup(t => t.FindAsync(It.IsAny<FilterDefinition<SurLaTableDocument>>(), It.IsAny<FindOptions<SurLaTableDocument, SurLaTableDocument>>(), It.IsAny<CancellationToken>())).ReturnsAsync(asyncCursor);

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
