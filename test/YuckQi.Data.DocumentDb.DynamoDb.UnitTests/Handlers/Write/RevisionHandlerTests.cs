using Amazon.DynamoDBv2.DataModel;
using Moq;
using NUnit.Framework;
using YuckQi.Data.DocumentDb.DynamoDb.Handlers.Write;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.DocumentDb.DynamoDb.UnitTests.Handlers.Write;

public class RevisionHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IDynamoDBContext>();

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void OptionsConstructor_CreatesInstance()
    {
        var options = new RevisionOptions();
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IDynamoDBContext>(options);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void MapperConstructor_CreatesInstance()
    {
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IDynamoDBContext, SurLaTableDocument>(null, new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Revise_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IDynamoDBContext>();
        var entity = new SurLaTableDocument { Identifier = 1, Name = "ABC" };

        Assert.Throws<ArgumentNullException>(Act);

        return;

        void Act() => handler.Revise(entity, null);
    }

    [Test]
    public void Revise_Batch_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IDynamoDBContext>();
        var entities = new[] { new SurLaTableDocument { Identifier = 1, Name = "ABC" } };

        Assert.Throws<ArgumentNullException>(Act);

        return;

        void Act() => handler.Revise(entities, null);
    }

    [Test]
    public void Revise_Async_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IDynamoDBContext>();
        var entity = new SurLaTableDocument { Identifier = 1, Name = "ABC" };

        Assert.ThrowsAsync<ArgumentNullException>(Act);

        return;

        Task Act() => handler.Revise(entity, null, CancellationToken.None);
    }

    [Test]
    public void Revise_Batch_Async_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RevisionHandler<SurLaTableDocument, Int32, IDynamoDBContext>();
        var entities = new[] { new SurLaTableDocument { Identifier = 1, Name = "ABC" } };

        Assert.ThrowsAsync<ArgumentNullException>(Act);

        return;

        Task Act() => handler.Revise(entities, null, CancellationToken.None);
    }

    public class SurLaTableDocument : IDomainEntity<Int32>, IRevisionMoment
    {
        public Int32 Identifier { get; set; }

        public String Name { get; set; } = String.Empty;

        public DateTimeOffset RevisionMoment { get; set; }
    }
}
