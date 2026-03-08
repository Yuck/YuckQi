using MongoDB.Driver;
using Moq;
using NUnit.Framework;

namespace YuckQi.Data.DocumentDb.MongoDb.UnitTests;

public class UnitOfWorkTests
{
    [Test]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new UnitOfWork(null!));
    }

    [Test]
    public void Constructor_WithClient_CreatesInstance()
    {
        var client = new Mock<IMongoClient>();
        var uow = new UnitOfWork(client.Object);

        Assert.That(uow, Is.Not.Null);
    }

    [Test]
    public void Scope_ReturnsClientSession()
    {
        var session = new Mock<IClientSessionHandle>();
        var client = new Mock<IMongoClient>();

        client.Setup(t => t.StartSession(It.IsAny<ClientSessionOptions>(), CancellationToken.None)).Returns(session.Object);

        var uow = new UnitOfWork(client.Object);

        Assert.That(uow.Scope, Is.SameAs(session.Object));
    }

    [Test]
    public void Dispose_WhenInTransaction_AbortsTransaction()
    {
        var session = new Mock<IClientSessionHandle>();
        var client = new Mock<IMongoClient>();

        session.Setup(t => t.IsInTransaction).Returns(true);
        client.Setup(t => t.StartSession(It.IsAny<ClientSessionOptions>(), CancellationToken.None)).Returns(session.Object);

        var uow = new UnitOfWork(client.Object);

        _ = uow.Scope;
        uow.Dispose();

        session.Verify(t => t.AbortTransaction(CancellationToken.None), Times.Once);
        session.Verify(t => t.Dispose(), Times.Once);
    }

    [Test]
    public void Dispose_WhenNotInTransaction_DisposesWithoutAborting()
    {
        var session = new Mock<IClientSessionHandle>();
        var client = new Mock<IMongoClient>();

        session.Setup(t => t.IsInTransaction).Returns(false);
        client.Setup(t => t.StartSession(It.IsAny<ClientSessionOptions>(), CancellationToken.None)).Returns(session.Object);

        var uow = new UnitOfWork(client.Object);

        _ = uow.Scope;
        uow.Dispose();

        session.Verify(t => t.AbortTransaction(CancellationToken.None), Times.Never);
        session.Verify(t => t.Dispose(), Times.Once);
    }

    [Test]
    public void Dispose_WhenCalledTwice_SecondCallIsNoOp()
    {
        var session = new Mock<IClientSessionHandle>();
        var client = new Mock<IMongoClient>();

        session.Setup(t => t.IsInTransaction).Returns(false);
        client.Setup(t => t.StartSession(It.IsAny<ClientSessionOptions>(), CancellationToken.None)).Returns(session.Object);

        var uow = new UnitOfWork(client.Object);

        _ = uow.Scope;
        uow.Dispose();
        uow.Dispose();

        session.Verify(t => t.Dispose(), Times.Once);
    }

    [Test]
    public void SaveChanges_WhenInTransaction_CommitsTransaction()
    {
        var session = new Mock<IClientSessionHandle>();
        var client = new Mock<IMongoClient>();

        session.Setup(t => t.IsInTransaction).Returns(true);
        client.Setup(t => t.StartSession(It.IsAny<ClientSessionOptions>(), CancellationToken.None)).Returns(session.Object);

        var uow = new UnitOfWork(client.Object);

        _ = uow.Scope;
        uow.SaveChanges();

        session.Verify(t => t.CommitTransaction(CancellationToken.None), Times.Once);
    }

    [Test]
    public void SaveChanges_WhenNotInTransaction_DoesNotCommit()
    {
        var session = new Mock<IClientSessionHandle>();
        var client = new Mock<IMongoClient>();

        session.Setup(t => t.IsInTransaction).Returns(false);
        client.Setup(t => t.StartSession(It.IsAny<ClientSessionOptions>(), CancellationToken.None)).Returns(session.Object);

        var uow = new UnitOfWork(client.Object);

        _ = uow.Scope;
        uow.SaveChanges();

        session.Verify(t => t.CommitTransaction(CancellationToken.None), Times.Never);
    }

    [Test]
    public void SaveChanges_AfterDispose_ThrowsInvalidOperationException()
    {
        var session = new Mock<IClientSessionHandle>();
        var client = new Mock<IMongoClient>();

        session.Setup(t => t.IsInTransaction).Returns(false);
        client.Setup(t => t.StartSession(It.IsAny<ClientSessionOptions>(), CancellationToken.None)).Returns(session.Object);

        var uow = new UnitOfWork(client.Object);

        _ = uow.Scope;
        uow.Dispose();

        Assert.Throws<InvalidOperationException>(() => uow.SaveChanges());
    }

    [Test]
    public void Scope_AfterDispose_ThrowsInvalidOperationException()
    {
        var session = new Mock<IClientSessionHandle>();
        var client = new Mock<IMongoClient>();

        session.Setup(t => t.IsInTransaction).Returns(false);
        client.Setup(t => t.StartSession(It.IsAny<ClientSessionOptions>(), CancellationToken.None)).Returns(session.Object);

        var uow = new UnitOfWork(client.Object);

        _ = uow.Scope;
        uow.Dispose();

        Assert.Throws<InvalidOperationException>(() => _ = uow.Scope);
    }

    [Test]
    public void SaveChanges_CreatesNewSession()
    {
        var session1 = new Mock<IClientSessionHandle>();
        var session2 = new Mock<IClientSessionHandle>();
        var client = new Mock<IMongoClient>();

        session1.Setup(t => t.IsInTransaction).Returns(false);
        client.SetupSequence(t => t.StartSession(It.IsAny<ClientSessionOptions>(), CancellationToken.None))
              .Returns(session1.Object)
              .Returns(session2.Object);

        var uow = new UnitOfWork(client.Object);
        var first = uow.Scope;

        uow.SaveChanges();
        var second = uow.Scope;

        Assert.That(second, Is.SameAs(session2.Object));
    }

    [Test]
    public void Constructor_WithOptions_PassesOptionsToStartSession()
    {
        var options = new ClientSessionOptions();
        var session = new Mock<IClientSessionHandle>();
        var client = new Mock<IMongoClient>();

        client.Setup(t => t.StartSession(options, CancellationToken.None)).Returns(session.Object);

        var uow = new UnitOfWork(client.Object, options);

        _ = uow.Scope;

        client.Verify(t => t.StartSession(options, CancellationToken.None), Times.Once);
    }
}
