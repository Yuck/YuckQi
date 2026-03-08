using System.Data;
using Dapper;
using Moq;
using NUnit.Framework;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Data.Sql.Dapper.Handlers.Write;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.Sql.Dapper.UnitTests.Handlers.Write;

public class RevisionHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new RevisionHandler<SurLaTableEntity, Int32, IDbTransaction>(null);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void ConstructorWithOptions_CreatesInstance()
    {
        var options = new RevisionOptions(PropertyHandling.Auto);
        var handler = new RevisionHandler<SurLaTableEntity, Int32, IDbTransaction>(options);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void MapperConstructor_CreatesInstance()
    {
        var handler = new RevisionHandler<SurLaTableEntity, Int32, IDbTransaction, SurLaTableEntity>(null, new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Revise_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RevisionHandler<SurLaTableEntity, Int32, IDbTransaction>(null);
        var entity = new SurLaTableEntity { Identifier = 1, Name = "ABC" };

        Assert.Throws<ArgumentNullException>(() => handler.Revise(entity, null));
    }

    [Test]
    public void ReviseAsync_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RevisionHandler<SurLaTableEntity, Int32, IDbTransaction>(null);
        var entity = new SurLaTableEntity { Identifier = 1, Name = "ABC" };

        Assert.ThrowsAsync<ArgumentNullException>(() => handler.Revise(entity, null, CancellationToken.None));
    }

    [Test]
    public void Revise_WithMockScope_ReturnsEntity()
    {
        var handler = new RevisionHandler<SurLaTableEntity, Int32, IDbTransaction>(null);
        var entity = new SurLaTableEntity { Identifier = 1, Name = "ABC" };
        var transaction = CreateMockTransaction();

        var result = handler.Revise(entity, transaction);

        Assert.That(result, Is.EqualTo(entity));
    }

    [Test]
    public async Task ReviseAsync_WithMockScope_ReturnsEntity()
    {
        var handler = new RevisionHandler<SurLaTableEntity, Int32, IDbTransaction>(null);
        var entity = new SurLaTableEntity { Identifier = 1, Name = "ABC" };
        var transaction = CreateMockTransaction();

        var result = await handler.Revise(entity, transaction, CancellationToken.None);

        Assert.That(result, Is.EqualTo(entity));
    }

    private static IDbTransaction CreateMockTransaction()
    {
        return MockDbFactory.CreateTransaction();
    }

    [Table("SurLaTable")]
    public record SurLaTableEntity : DomainEntityBase<Int32>, IRevisionMoment
    {
        [Key]
        public Int32 Id { get => Identifier; set => Identifier = value; }

        public String Name { get; set; } = String.Empty;

        public DateTimeOffset RevisionMoment { get; set; }
    }
}
