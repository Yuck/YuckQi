using Amazon.DynamoDBv2.DataModel;
using Moq;
using NUnit.Framework;

namespace YuckQi.Data.DocumentDb.DynamoDb.UnitTests;

public class UnitOfWorkTests
{
    [Test]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(Act);

        return;

        void Act() => new UnitOfWork(null!);
    }

    [Test]
    public void Constructor_WithContext_CreatesInstance()
    {
        var context = new Mock<IDynamoDBContext>();
        var uow = new UnitOfWork(context.Object);

        Assert.That(uow, Is.Not.Null);
    }

    [Test]
    public void Scope_ReturnsContext()
    {
        var context = new Mock<IDynamoDBContext>();
        var uow = new UnitOfWork(context.Object);

        Assert.That(uow.Scope, Is.SameAs(context.Object));
    }

    [Test]
    public void Dispose_DisposesContext()
    {
        var context = new Mock<IDynamoDBContext>();
        var uow = new UnitOfWork(context.Object);

        uow.Dispose();

        context.Verify(t => t.Dispose(), Times.Once);
    }

    [Test]
    public void Dispose_SetsScope_ToNull()
    {
        var context = new Mock<IDynamoDBContext>();
        var uow = new UnitOfWork(context.Object);

        uow.Dispose();

        Assert.That(uow.Scope, Is.Null);
    }

    [Test]
    public void Dispose_WhenCalledTwice_SecondCallIsNoOp()
    {
        var context = new Mock<IDynamoDBContext>();
        var uow = new UnitOfWork(context.Object);

        uow.Dispose();
        uow.Dispose();

        context.Verify(t => t.Dispose(), Times.Once);
    }

    [Test]
    public void Dispose_WhenScopeIsNull_DoesNotThrow()
    {
        var context = new Mock<IDynamoDBContext>();
        var uow = new UnitOfWork(context.Object);

        uow.Dispose();

        Assert.DoesNotThrow(uow.Dispose);
    }

    [Test]
    public void SaveChanges_ThrowsNotImplementedException()
    {
        var context = new Mock<IDynamoDBContext>();
        var uow = new UnitOfWork(context.Object);

        Assert.Throws<NotImplementedException>(uow.SaveChanges);
    }
}
