using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using YuckQi.Data.DocumentDb.MongoDb.Attributes;
using YuckQi.Data.DocumentDb.MongoDb.Handlers.Write;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.DocumentDb.MongoDb.UnitTests.Handlers.Write;

public class CreationHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new CreationHandler<SurLaTableDocument, Int32, IClientSessionHandle>();

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void OptionsConstructor_CreatesInstance()
    {
        var options = new CreationOptions<Int32>();
        var handler = new CreationHandler<SurLaTableDocument, Int32, IClientSessionHandle>(options);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void MapperConstructor_CreatesInstance()
    {
        var handler = new CreationHandler<SurLaTableDocument, Int32, IClientSessionHandle, SurLaTableDocument>(null, new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Create_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new CreationHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var entity = new SurLaTableDocument { Identifier = 1, Name = "ABC" };

        Assert.Throws<ArgumentNullException>(() => handler.Create(entity, null));
    }

    [Test]
    public void Create_Batch_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new CreationHandler<SurLaTableDocument, Int32, IClientSessionHandle>();
        var entities = new[] { new SurLaTableDocument { Identifier = 1, Name = "ABC" } };

        Assert.Throws<ArgumentNullException>(() => handler.Create(entities, null));
    }

    [Test]
    public void Create_WithScope_ReturnsEntity()
    {
        var scope = CreateMockScope();
        var options = new CreationOptions<Int32>(identifierFactory: () => 42);
        var handler = new CreationHandler<SurLaTableDocument, Int32, IClientSessionHandle>(options);
        var entity = new SurLaTableDocument { Name = "ABC" };

        var result = handler.Create(entity, scope.Object);

        Assert.That(result.Identifier, Is.EqualTo(42));
    }

    [Test]
    public async Task Create_Async_ReturnsEntity()
    {
        var scope = CreateMockScope();
        var options = new CreationOptions<Int32>(identifierFactory: () => 42);
        var handler = new CreationHandler<SurLaTableDocument, Int32, IClientSessionHandle>(options);
        var entity = new SurLaTableDocument { Name = "ABC" };

        var result = await handler.Create(entity, scope.Object, CancellationToken.None);

        Assert.That(result.Identifier, Is.EqualTo(42));
    }

    [Test]
    public void Create_Batch_ReturnsEntities()
    {
        var scope = CreateMockScope();
        var counter = 0;
        var options = new CreationOptions<Int32>(identifierFactory: () => ++counter);
        var handler = new CreationHandler<SurLaTableDocument, Int32, IClientSessionHandle>(options);
        var entities = new[] { new SurLaTableDocument { Name = "ABC" }, new SurLaTableDocument { Name = "DEF" } };

        var result = handler.Create(entities, scope.Object).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Create_Batch_Async_ReturnsEntities()
    {
        var scope = CreateMockScope();
        var counter = 0;
        var options = new CreationOptions<Int32>(identifierFactory: () => ++counter);
        var handler = new CreationHandler<SurLaTableDocument, Int32, IClientSessionHandle>(options);
        var entities = new[] { new SurLaTableDocument { Name = "ABC" }, new SurLaTableDocument { Name = "DEF" } };

        var result = (await handler.Create(entities, scope.Object, CancellationToken.None)).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public void Create_WithAutoCreationMoment_SetsCreationMoment()
    {
        var scope = CreateMockScope();
        var options = new CreationOptions<Int32>(identifierFactory: () => 1, creationMomentAssignment: PropertyHandling.Auto);
        var handler = new CreationHandler<SurLaTableDocument, Int32, IClientSessionHandle>(options);
        var entity = new SurLaTableDocument { Name = "ABC" };

        var result = handler.Create(entity, scope.Object);

        Assert.That(result.CreationMoment, Is.Not.EqualTo(default(DateTimeOffset)));
    }

    private static Mock<IClientSessionHandle> CreateMockScope()
    {
        var collection = new Mock<IMongoCollection<SurLaTableDocument>>();

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
    public class SurLaTableDocument : IDomainEntity<Int32>, ICreationMoment
    {
        public DateTimeOffset CreationMoment { get; set; }

        [BsonId] public Int32 Identifier { get; set; }

        public String Name { get; set; } = String.Empty;
    }
}
