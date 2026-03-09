using System.Data;
using Dapper;
using Moq;
using NUnit.Framework;
using YuckQi.Data.Filtering;
using YuckQi.Data.Sorting;
using YuckQi.Data.Sql.Dapper.SqlServer.Handlers.Read;
using YuckQi.Domain.Entities.Abstract;
using YuckQi.Domain.ValueObjects;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.Sql.Dapper.SqlServer.UnitTests.Handlers.Read;

public class SearchHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new SearchHandler<SurLaTableEntity, Int32, IDbTransaction>();

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void SqlGeneratorConstructor_CreatesInstance()
    {
        var generator = new SqlGenerator<SurLaTableEntity>();
        var handler = new SearchHandler<SurLaTableEntity, Int32, IDbTransaction>(generator);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void MapperConstructor_CreatesInstance()
    {
        var handler = new SearchHandler<SurLaTableEntity, Int32, IDbTransaction, SurLaTableEntity>(new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void SqlGeneratorAndMapperConstructor_CreatesInstance()
    {
        var generator = new SqlGenerator<SurLaTableEntity>();
        var handler = new SearchHandler<SurLaTableEntity, Int32, IDbTransaction, SurLaTableEntity>(generator, new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Search_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new SearchHandler<SurLaTableEntity, Int32, IDbTransaction>();
        var parameters = new[] { new FilterCriteria("Name", "ABC") };
        var page = new Page(1, 10);
        var sort = new[] { new SortCriteria("Name", SortOrder.Descending) }.OrderBy(_ => 1);

        Assert.Throws<ArgumentNullException>(() => handler.Search(parameters, page, sort, null));
    }

    [Table("SurLaTable")]
    public record SurLaTableEntity : DomainEntityBase<Int32>
    {
        public String Name { get; set; } = String.Empty;
    }
}
