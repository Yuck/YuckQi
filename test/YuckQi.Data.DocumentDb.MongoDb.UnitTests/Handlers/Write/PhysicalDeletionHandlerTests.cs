using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using YuckQi.Data.DocumentDb.MongoDb.Attributes;
using YuckQi.Data.DocumentDb.MongoDb.Handlers.Write;
using YuckQi.Data.Exceptions;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.DocumentDb.MongoDb.UnitTests.Handlers.Write;

public class PhysicalDeletionHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new PhysicalDeletionHandler<SurLaTableDocument, Int32, IClientSessionHandle>();

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void MapperConstructor_CreatesInstance()
    {
        var handler = new PhysicalDeletionHandler<SurLaTableDocument, Int32, IClientSessionHandle, SurLaTableDocument>(new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Delete_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new PhysicalDeletionHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var entity = new SurLaTableDocument { Identifier = 1, Name = "ABC" };

        Assert.Throws<ArgumentNullException>(() => handler.Delete(entity, null));
    }

    [Test]
    public void Delete_WithScope_ReturnsEntity()
    {
        var scope = CreateMockScope(deletedCount: 1);
        var handler = new PhysicalDeletionHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var entity = new SurLaTableDocument { Identifier = 1, Name = "ABC" };

        var result = handler.Delete(entity, scope.Object);

        Assert.That(result.Identifier, Is.EqualTo(1));
    }

    [Test]
    public async Task Delete_Async_ReturnsEntity()
    {
        var scope = CreateMockScope(deletedCount: 1, async: true);
        var handler = new PhysicalDeletionHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var entity = new SurLaTableDocument { Identifier = 1, Name = "ABC" };

        var result = await handler.Delete(entity, scope.Object, CancellationToken.None);

        Assert.That(result.Identifier, Is.EqualTo(1));
    }

    [Test]
    public void Delete_WhenNotFound_ThrowsException()
    {
        var scope = CreateMockScope(deletedCount: 0);
        var handler = new PhysicalDeletionHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var entity = new SurLaTableDocument { Identifier = 1, Name = "ABC" };

        Assert.Throws<PhysicalDeletionException<SurLaTableDocument, Int32>>(() => handler.Delete(entity, scope.Object));
    }

    private static Mock<IClientSessionHandle> CreateMockScope(Int64 deletedCount, Boolean async = false)
    {
        var deleteResult = new Mock<DeleteResult>();
        deleteResult.Setup(t => t.DeletedCount).Returns(deletedCount);

        var collection = new Mock<IMongoCollection<SurLaTableDocument>>();
        collection.Setup(t => t.DeleteOne(It.IsAny<IClientSessionHandle>(), It.IsAny<FilterDefinition<SurLaTableDocument>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>())).Returns(deleteResult.Object);
        collection.Setup(t => t.DeleteOneAsync(It.IsAny<IClientSessionHandle>(), It.IsAny<FilterDefinition<SurLaTableDocument>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(deleteResult.Object);

        var database = new Mock<IMongoDatabase>();
        database.Setup(t => t.GetCollection<SurLaTableDocument>(It.IsAny<String>(), It.IsAny<MongoCollectionSettings>())).Returns(collection.Object);

        var client = new Mock<IMongoClient>();
        client.Setup(t => t.GetDatabase(It.IsAny<String>(), It.IsAny<MongoDatabaseSettings>())).Returns(database.Object);

        var scope = new Mock<IClientSessionHandle>();
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
