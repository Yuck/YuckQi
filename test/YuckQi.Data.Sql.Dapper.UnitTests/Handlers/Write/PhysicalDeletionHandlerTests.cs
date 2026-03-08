using System.Data;
using Dapper;
using Moq;
using NUnit.Framework;
using YuckQi.Data.Sql.Dapper.Handlers.Write;
using YuckQi.Domain.Entities.Abstract;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.Sql.Dapper.UnitTests.Handlers.Write;

public class PhysicalDeletionHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new PhysicalDeletionHandler<SurLaTableEntity, Int32, IDbTransaction>();

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void MapperConstructor_CreatesInstance()
    {
        var handler = new PhysicalDeletionHandler<SurLaTableEntity, Int32, IDbTransaction, SurLaTableEntity>(new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Delete_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new PhysicalDeletionHandler<SurLaTableEntity, Int32, IDbTransaction>();
        var entity = new SurLaTableEntity { Identifier = 1, Name = "ABC" };

        Assert.Throws<ArgumentNullException>(() => handler.Delete(entity, null));
    }

    [Test]
    public void DeleteAsync_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new PhysicalDeletionHandler<SurLaTableEntity, Int32, IDbTransaction>();
        var entity = new SurLaTableEntity { Identifier = 1, Name = "ABC" };

        Assert.ThrowsAsync<ArgumentNullException>(() => handler.Delete(entity, null, CancellationToken.None));
    }

    [Test]
    public void Delete_WithMockScope_ReturnsEntity()
    {
        var handler = new PhysicalDeletionHandler<SurLaTableEntity, Int32, IDbTransaction>();
        var entity = new SurLaTableEntity { Identifier = 1, Name = "ABC" };
        var transaction = CreateMockTransaction();

        var result = handler.Delete(entity, transaction);

        Assert.That(result, Is.EqualTo(entity));
    }

    [Test]
    public async Task DeleteAsync_WithMockScope_ReturnsEntity()
    {
        var handler = new PhysicalDeletionHandler<SurLaTableEntity, Int32, IDbTransaction>();
        var entity = new SurLaTableEntity { Identifier = 1, Name = "ABC" };
        var transaction = CreateMockTransaction();

        var result = await handler.Delete(entity, transaction, CancellationToken.None);

        Assert.That(result, Is.EqualTo(entity));
    }

    private static IDbTransaction CreateMockTransaction()
    {
        return MockDbFactory.CreateTransaction();
    }

    [Table("SurLaTable")]
    public record SurLaTableEntity : DomainEntityBase<Int32>
    {
        [Key]
        public Int32 Id { get => Identifier; set => Identifier = value; }

        public String Name { get; set; } = String.Empty;
    }
}
