using System.Data;
using Dapper;
using Moq;
using NUnit.Framework;
using YuckQi.Data.Sql.Dapper.Handlers.Write;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.Sql.Dapper.UnitTests.Handlers.Write;

public class CreationHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new CreationHandler<SurLaTableEntity, Int32, IDbTransaction>();

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void MapperConstructor_CreatesInstance()
    {
        var handler = new CreationHandler<SurLaTableEntity, Int32, IDbTransaction, SurLaTableEntity>(null, new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Create_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new CreationHandler<SurLaTableEntity, Int32, IDbTransaction>();
        var entity = new SurLaTableEntity { Identifier = 1, Name = "ABC" };

        Assert.Throws<ArgumentNullException>(() => handler.Create(entity, null));
    }

    [Test]
    public void CreateAsync_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new CreationHandler<SurLaTableEntity, Int32, IDbTransaction>();
        var entity = new SurLaTableEntity { Identifier = 1, Name = "ABC" };

        Assert.ThrowsAsync<ArgumentNullException>(() => handler.Create(entity, null, CancellationToken.None));
    }

    [Test]
    public void Create_WithMockScope_ThrowsCreationException()
    {
        var handler = new CreationHandler<SurLaTableEntity, Int32, IDbTransaction>();
        var entity = new SurLaTableEntity { Identifier = 0, Name = "ABC" };
        var transaction = CreateMockTransaction();

        Assert.Throws<InvalidOperationException>(() => handler.Create(entity, transaction));
    }

    [Test]
    public void CreateAsync_WithMockScope_ThrowsCreationException()
    {
        var handler = new CreationHandler<SurLaTableEntity, Int32, IDbTransaction>();
        var entity = new SurLaTableEntity { Identifier = 0, Name = "ABC" };
        var transaction = CreateMockTransaction();

        Assert.ThrowsAsync<InvalidOperationException>(() => handler.Create(entity, transaction, CancellationToken.None));
    }

    private static IDbTransaction CreateMockTransaction()
    {
        return MockDbFactory.CreateTransaction();
    }

    [Table("SurLaTable")]
    public record SurLaTableEntity : DomainEntityBase<Int32>, ICreationMoment
    {
        public DateTimeOffset CreationMoment { get; set; }

        [Key]
        public Int32 Id { get => Identifier; set => Identifier = value; }

        public String Name { get; set; } = String.Empty;
    }
}
