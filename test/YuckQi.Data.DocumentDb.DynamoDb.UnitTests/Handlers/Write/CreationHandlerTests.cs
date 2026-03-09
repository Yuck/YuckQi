using Amazon.DynamoDBv2.DataModel;
using Moq;
using NUnit.Framework;
using YuckQi.Data.DocumentDb.DynamoDb.Handlers.Write;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.DocumentDb.DynamoDb.UnitTests.Handlers.Write;

public class CreationHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new CreationHandler<SurLaTableDocument, Int32, IDynamoDBContext>();

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void OptionsConstructor_CreatesInstance()
    {
        var options = new CreationOptions<Int32>();
        var handler = new CreationHandler<SurLaTableDocument, Int32, IDynamoDBContext>(options);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void MapperConstructor_CreatesInstance()
    {
        var handler = new CreationHandler<SurLaTableDocument, Int32, IDynamoDBContext, SurLaTableDocument>(null, new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Create_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new CreationHandler<SurLaTableDocument, Int32, IDynamoDBContext>();
        var entity = new SurLaTableDocument { Identifier = 1, Name = "ABC" };

        Assert.Throws<ArgumentNullException>(Act);

        return;

        void Act() => handler.Create(entity, null);
    }

    [Test]
    public void Create_Batch_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new CreationHandler<SurLaTableDocument, Int32, IDynamoDBContext>();
        var entities = new[] { new SurLaTableDocument { Identifier = 1, Name = "ABC" } };

        Assert.Throws<ArgumentNullException>(Act);

        return;

        void Act() => handler.Create(entities, null);
    }

    [Test]
    public void Create_Async_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new CreationHandler<SurLaTableDocument, Int32, IDynamoDBContext>();
        var entity = new SurLaTableDocument { Identifier = 1, Name = "ABC" };

        Assert.ThrowsAsync<ArgumentNullException>(Act);

        return;

        Task Act() => handler.Create(entity, null, CancellationToken.None);
    }

    [Test]
    public void Create_Batch_Async_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new CreationHandler<SurLaTableDocument, Int32, IDynamoDBContext>();
        var entities = new[] { new SurLaTableDocument { Identifier = 1, Name = "ABC" } };

        Assert.ThrowsAsync<ArgumentNullException>(Act);

        return;

        Task Act() => handler.Create(entities, null, CancellationToken.None);
    }

    public class SurLaTableDocument : IDomainEntity<Int32>, ICreationMoment
    {
        public DateTimeOffset CreationMoment { get; set; }

        public Int32 Identifier { get; set; }

        public String Name { get; set; } = String.Empty;
    }
}
