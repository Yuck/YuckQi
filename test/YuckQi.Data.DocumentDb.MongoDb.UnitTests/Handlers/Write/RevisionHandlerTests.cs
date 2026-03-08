using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using YuckQi.Data.DocumentDb.MongoDb.Attributes;
using YuckQi.Data.DocumentDb.MongoDb.Handlers.Write;
using YuckQi.Data.Exceptions;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.DocumentDb.MongoDb.UnitTests.Handlers.Write;

public class RevisionHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IClientSessionHandle>();

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void OptionsConstructor_CreatesInstance()
    {
        var options = new RevisionOptions();
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IClientSessionHandle>(options);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void MapperConstructor_CreatesInstance()
    {
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IClientSessionHandle, SurLaTableDocument>(null, new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Revise_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var entity = new SurLaTableDocument { Identifier = 1, Name = "ABC" };

        Assert.Throws<ArgumentNullException>(() => handler.Revise(entity, null));
    }

    [Test]
    public void Revise_Batch_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var entities = new[] { new SurLaTableDocument { Identifier = 1, Name = "ABC" } };

        Assert.Throws<ArgumentNullException>(() => handler.Revise(entities, null));
    }

    [Test]
    public void Revise_WithScope_ReturnsEntity()
    {
        var scope = CreateMockScope(modifiedCount: 1, matchedCount: 1);
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var entity = new SurLaTableDocument { Identifier = 1, Name = "Updated" };

        var result = handler.Revise(entity, scope.Object);

        Assert.That(result.Identifier, Is.EqualTo(1));
    }

    [Test]
    public async Task Revise_Async_ReturnsEntity()
    {
        var scope = CreateMockScope(modifiedCount: 1, matchedCount: 1);
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var entity = new SurLaTableDocument { Identifier = 1, Name = "Updated" };

        var result = await handler.Revise(entity, scope.Object, CancellationToken.None);

        Assert.That(result.Identifier, Is.EqualTo(1));
    }

    [Test]
    public void Revise_WhenMatchedButNotModified_ReturnsEntity()
    {
        var scope = CreateMockScope(modifiedCount: 0, matchedCount: 1);
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var entity = new SurLaTableDocument { Identifier = 1, Name = "Same" };

        var result = handler.Revise(entity, scope.Object);

        Assert.That(result.Identifier, Is.EqualTo(1));
    }

    [Test]
    public void Revise_WhenNotFound_ThrowsException()
    {
        var scope = CreateMockScope(modifiedCount: 0, matchedCount: 0);
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var entity = new SurLaTableDocument { Identifier = 1, Name = "ABC" };

        Assert.Throws<RevisionException<SurLaTableDocument, Int32>>(() => handler.Revise(entity, scope.Object));
    }

    [Test]
    public void Revise_Batch_ReturnsEntities()
    {
        var scope = CreateMockScope(modifiedCount: 1, matchedCount: 1);
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var entities = new[] { new SurLaTableDocument { Identifier = 1, Name = "ABC" }, new SurLaTableDocument { Identifier = 2, Name = "DEF" } };

        var result = handler.Revise(entities, scope.Object).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Revise_Batch_Async_ReturnsEntities()
    {
        var scope = CreateMockScope(modifiedCount: 1, matchedCount: 1);
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var entities = new[] { new SurLaTableDocument { Identifier = 1, Name = "ABC" }, new SurLaTableDocument { Identifier = 2, Name = "DEF" } };

        var result = (await handler.Revise(entities, scope.Object, CancellationToken.None)).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public void Revise_WithAutoRevisionMoment_SetsRevisionMoment()
    {
        var scope = CreateMockScope(modifiedCount: 1, matchedCount: 1);
        var options = new RevisionOptions(PropertyHandling.Auto);
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IClientSessionHandle>(options);
        var entity = new SurLaTableDocument { Identifier = 1, Name = "ABC" };

        var result = handler.Revise(entity, scope.Object);

        Assert.That(result.RevisionMoment, Is.Not.EqualTo(default(DateTimeOffset)));
    }

    private static Mock<IClientSessionHandle> CreateMockScope(Int64 modifiedCount, Int64 matchedCount)
    {
        var replaceResult = new Mock<ReplaceOneResult>();
        replaceResult.Setup(t => t.ModifiedCount).Returns(modifiedCount);
        replaceResult.Setup(t => t.MatchedCount).Returns(matchedCount);
        replaceResult.Setup(t => t.IsAcknowledged).Returns(true);

        var collection = new Mock<IMongoCollection<SurLaTableDocument>>();
        collection.Setup(t => t.ReplaceOne(It.IsAny<IClientSessionHandle>(), It.IsAny<FilterDefinition<SurLaTableDocument>>(), It.IsAny<SurLaTableDocument>(), It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>())).Returns(replaceResult.Object);
        collection.Setup(t => t.ReplaceOneAsync(It.IsAny<IClientSessionHandle>(), It.IsAny<FilterDefinition<SurLaTableDocument>>(), It.IsAny<SurLaTableDocument>(), It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(replaceResult.Object);

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
    public class SurLaTableDocument : IDomainEntity<Int32>, IRevisionMoment
    {
        [BsonId] public Int32 Identifier { get; set; }

        public String Name { get; set; } = String.Empty;

        public DateTimeOffset RevisionMoment { get; set; }
    }
}
