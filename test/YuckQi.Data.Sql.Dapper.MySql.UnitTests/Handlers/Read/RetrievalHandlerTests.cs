using System.Data;
using Dapper;
using Moq;
using NUnit.Framework;
using YuckQi.Data.Filtering;
using YuckQi.Data.Sql.Dapper.MySql.Handlers.Read;
using YuckQi.Domain.Entities.Abstract;
using YuckQi.Extensions.Mapping.Abstractions;

namespace YuckQi.Data.Sql.Dapper.MySql.UnitTests.Handlers.Read;

public class RetrievalHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new RetrievalHandler<SurLaTableEntity, Int32, IDbTransaction>();

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void SqlGeneratorConstructor_CreatesInstance()
    {
        var generator = new SqlGenerator<SurLaTableEntity>();
        var handler = new RetrievalHandler<SurLaTableEntity, Int32, IDbTransaction>(generator);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void MapperConstructor_CreatesInstance()
    {
        var handler = new RetrievalHandler<SurLaTableEntity, Int32, IDbTransaction, SurLaTableEntity>(new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void SqlGeneratorAndMapperConstructor_CreatesInstance()
    {
        var generator = new SqlGenerator<SurLaTableEntity>();
        var handler = new RetrievalHandler<SurLaTableEntity, Int32, IDbTransaction, SurLaTableEntity>(generator, new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Get_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableEntity, Int32, IDbTransaction>();

        Assert.Throws<ArgumentNullException>(() => handler.Get(1, null));
    }

    [Test]
    public void Get_WithFilterCriteria_AndNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableEntity, Int32, IDbTransaction>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        Assert.Throws<ArgumentNullException>(() => handler.Get(parameters, null));
    }

    [Test]
    public void GetList_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableEntity, Int32, IDbTransaction>();

        Assert.Throws<ArgumentNullException>(() => handler.GetList(null));
    }

    [Table("SurLaTable")]
    public record SurLaTableEntity : DomainEntityBase<Int32>
    {
        public String Name { get; set; } = String.Empty;
    }
}
