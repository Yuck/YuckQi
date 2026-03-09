using Moq;
using NUnit.Framework;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using YuckQi.Data.DocumentDb.RavenDb;

namespace YuckQi.Data.DocumentDb.RavenDb.UnitTests;

public class UnitOfWorkTests
{
    [Test]
    public void Constructor_WithNullStore_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new UnitOfWork(null!));
    }

    [Test]
    public void Constructor_WithStore_CreatesInstance()
    {
        var store = new Mock<IDocumentStore>();
        var session = new Mock<IAsyncDocumentSession>();

        store.Setup(t => t.OpenAsyncSession(It.IsAny<String>())).Returns(session.Object);
        store.Setup(t => t.OpenAsyncSession()).Returns(session.Object);

        var uow = new UnitOfWork(store.Object);

        Assert.That(uow, Is.Not.Null);
    }

    [Test]
    public void Scope_ReturnsSession()
    {
        var session = new Mock<IAsyncDocumentSession>();
        var store = new Mock<IDocumentStore>();

        store.Setup(t => t.OpenAsyncSession()).Returns(session.Object);

        var uow = new UnitOfWork(store.Object);

        Assert.That(uow.Scope, Is.SameAs(session.Object));
    }

    [Test]
    public void Constructor_WithDatabase_OpensSessionForDatabase()
    {
        var session = new Mock<IAsyncDocumentSession>();
        var store = new Mock<IDocumentStore>();

        store.Setup(t => t.OpenAsyncSession("TestDb")).Returns(session.Object);

        var uow = new UnitOfWork(store.Object, "TestDb");

        _ = uow.Scope;

        store.Verify(t => t.OpenAsyncSession("TestDb"), Times.Once);
    }

    [Test]
    public void Dispose_DisposesSession()
    {
        var session = new Mock<IAsyncDocumentSession>();
        var store = new Mock<IDocumentStore>();

        store.Setup(t => t.OpenAsyncSession()).Returns(session.Object);

        var uow = new UnitOfWork(store.Object);

        _ = uow.Scope;

        uow.Dispose();

        session.Verify(t => t.Dispose(), Times.Once);
    }

    [Test]
    public void SaveChanges_CallsSaveChangesAsync()
    {
        var session = new Mock<IAsyncDocumentSession>();
        var store = new Mock<IDocumentStore>();

        session.Setup(t => t.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        store.Setup(t => t.OpenAsyncSession()).Returns(session.Object);

        var uow = new UnitOfWork(store.Object);

        _ = uow.Scope;

        uow.SaveChanges();

        session.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void SaveChanges_AfterDispose_ThrowsInvalidOperationException()
    {
        var session = new Mock<IAsyncDocumentSession>();
        var store = new Mock<IDocumentStore>();

        store.Setup(t => t.OpenAsyncSession()).Returns(session.Object);

        var uow = new UnitOfWork(store.Object);

        _ = uow.Scope;

        uow.Dispose();

        Assert.Throws<InvalidOperationException>(() => uow.SaveChanges());
    }

    [Test]
    public async Task SaveChanges_WithCancellationToken_CallsSaveChangesAsync()
    {
        var session = new Mock<IAsyncDocumentSession>();
        var store = new Mock<IDocumentStore>();

        session.Setup(t => t.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        store.Setup(t => t.OpenAsyncSession()).Returns(session.Object);

        var uow = new UnitOfWork(store.Object);

        _ = uow.Scope;

        await uow.SaveChanges(CancellationToken.None);

        session.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
