using Amazon.DynamoDBv2.DataModel;
using Moq;
using NUnit.Framework;
using YuckQi.Data.DocumentDb.DynamoDb.Handlers.Write;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.DocumentDb.DynamoDb.UnitTests.Handlers.Write;

public class PhysicalDeletionHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new PhysicalDeletionHandler<SurLaTableDocument, Int32, IDynamoDBContext>();

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void MapperConstructor_CreatesInstance()
    {
        var handler = new PhysicalDeletionHandler<SurLaTableDocument, Int32, IDynamoDBContext, SurLaTableDocument>(new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Delete_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new PhysicalDeletionHandler<SurLaTableDocument, Int32, IDynamoDBContext>();
        var entity = new SurLaTableDocument { Identifier = 1, Name = "ABC" };

        Assert.Throws<ArgumentNullException>(Act);

        return;

        void Act() => handler.Delete(entity, null);
    }

    [Test]
    public void Delete_Async_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new PhysicalDeletionHandler<SurLaTableDocument, Int32, IDynamoDBContext>();
        var entity = new SurLaTableDocument { Identifier = 1, Name = "ABC" };

        Assert.ThrowsAsync<ArgumentNullException>(Act);

        return;

        Task Act() => handler.Delete(entity, null, CancellationToken.None);
    }

    public class SurLaTableDocument : IDomainEntity<Int32>
    {
        public Int32 Identifier { get; set; }

        public String Name { get; set; } = String.Empty;
    }
}
