using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Moq;
using NUnit.Framework;
using YuckQi.Data.DocumentDb.DynamoDb.Handlers.Read;
using YuckQi.Data.Filtering;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.DocumentDb.DynamoDb.UnitTests.Handlers.Read;

public class RetrievalHandlerTests
{
    [Test]
    public void DefaultConstructor_CreatesInstance()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IDynamoDBContext>(HashKeyFactory);

        Assert.That(handler, Is.Not.Null);

        return;

        Primitive HashKeyFactory(Int32 t) => t;
    }

    [Test]
    public void RangeKeyConstructor_CreatesInstance()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IDynamoDBContext>(HashKeyFactory, RangeKeyFactory);

        Assert.That(handler, Is.Not.Null);

        return;

        Primitive HashKeyFactory(Int32 t) => t;
        Primitive RangeKeyFactory(Int32 t) => t;
    }

    [Test]
    public void MapperConstructor_CreatesInstance()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IDynamoDBContext, SurLaTableDocument>(HashKeyFactory, null, new Mock<IMapper>().Object);

        Assert.That(handler, Is.Not.Null);

        return;

        Primitive HashKeyFactory(Int32 t) => t;
    }

    [Test]
    public void Get_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IDynamoDBContext>(HashKeyFactory);

        Assert.Throws<ArgumentNullException>(Act);

        return;

        void Act() => handler.Get(1, null);
        Primitive HashKeyFactory(Int32 t) => t;
    }

    [Test]
    public void Get_WithFilterCriteria_AndNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IDynamoDBContext>(HashKeyFactory);
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        Assert.Throws<ArgumentNullException>(Act);

        return;

        void Act() => handler.Get(parameters, null);
        Primitive HashKeyFactory(Int32 t) => t;
    }

    [Test]
    public void GetList_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IDynamoDBContext>(HashKeyFactory);

        Assert.Throws<ArgumentNullException>(Act);

        return;

        void Act() => handler.GetList(null);
        Primitive HashKeyFactory(Int32 t) => t;
    }

    [Test]
    public void GetList_WithFilterCriteria_AndNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IDynamoDBContext>(HashKeyFactory);
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        Assert.Throws<ArgumentNullException>(Act);

        return;

        void Act() => handler.GetList(parameters, null);
        Primitive HashKeyFactory(Int32 t) => t;
    }

    [Test]
    public void Get_Async_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IDynamoDBContext>(HashKeyFactory);

        Assert.ThrowsAsync<ArgumentNullException>(Act);

        return;

        Task Act() => handler.Get(1, null, CancellationToken.None);
        Primitive HashKeyFactory(Int32 t) => t;
    }

    [Test]
    public void Get_Async_WithFilterCriteria_AndNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IDynamoDBContext>(HashKeyFactory);
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        Assert.ThrowsAsync<ArgumentNullException>(Act);

        return;

        Task Act() => handler.Get(parameters, null, CancellationToken.None);
        Primitive HashKeyFactory(Int32 t) => t;
    }

    [Test]
    public void GetList_Async_WithNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IDynamoDBContext>(HashKeyFactory);

        Assert.ThrowsAsync<ArgumentNullException>(Act);

        return;

        Task Act() => handler.GetList(null, CancellationToken.None);
        Primitive HashKeyFactory(Int32 t) => t;
    }

    [Test]
    public void GetList_Async_WithFilterCriteria_AndNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IDynamoDBContext>(HashKeyFactory);
        var parameters = new[] { new FilterCriteria("Name", "ABC") };

        Assert.ThrowsAsync<ArgumentNullException>(Act);

        return;

        Task Act() => handler.GetList(parameters, null, CancellationToken.None);
        Primitive HashKeyFactory(Int32 t) => t;
    }

    [Test]
    public void Get_WithObjectParameters_AndNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IDynamoDBContext>(HashKeyFactory);
        var parameters = new { Name = "ABC" };

        Assert.Throws<ArgumentNullException>(Act);

        return;

        void Act() => handler.Get(parameters, null);
        Primitive HashKeyFactory(Int32 t) => t;
    }

    [Test]
    public void Get_Async_WithObjectParameters_AndNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IDynamoDBContext>(HashKeyFactory);
        var parameters = new { Name = "ABC" };

        Assert.ThrowsAsync<ArgumentNullException>(Act);

        return;

        Task Act() => handler.Get(parameters, null, CancellationToken.None);
        Primitive HashKeyFactory(Int32 t) => t;
    }

    [Test]
    public void GetList_WithObjectParameters_AndNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IDynamoDBContext>(HashKeyFactory);
        var parameters = new { Name = "ABC" };

        Assert.Throws<ArgumentNullException>(Act);

        return;

        void Act() => handler.GetList(parameters, null);
        Primitive HashKeyFactory(Int32 t) => t;
    }

    [Test]
    public void GetList_Async_WithObjectParameters_AndNullScope_ThrowsArgumentNullException()
    {
        var handler = new RetrievalHandler<SurLaTableDocument, Int32, IDynamoDBContext>(HashKeyFactory);
        var parameters = new { Name = "ABC" };

        Assert.ThrowsAsync<ArgumentNullException>(Act);

        return;

        Task Act() => handler.GetList(parameters, null, CancellationToken.None);
        Primitive HashKeyFactory(Int32 t) => t;
    }

    public class SurLaTableDocument : IDomainEntity<Int32>
    {
        public Int32 Identifier { get; set; }

        public String Name { get; set; } = String.Empty;
    }
}
