using Moq;
using NUnit.Framework;
using Raven.Client.Documents.Session;
using YuckQi.Data.DocumentDb.RavenDb.Handlers.Write;
using YuckQi.Data.Exceptions;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract;

namespace YuckQi.Data.DocumentDb.RavenDb.UnitTests.Handlers.Write;

public class CreationHandlerTests
{
    [Test]
    public async Task Create_WithStringIdentifier_StoresDocumentWithIdentifier()
    {
        var session = new Mock<IAsyncDocumentSession>();
        var handler = new CreationHandler<StringDocument, String, IAsyncDocumentSession>();
        var entity = new StringDocument { Identifier = "surlatables/abc" };
        String? storedId = null;

        session.Setup(t => t.StoreAsync(It.IsAny<Object>(), It.IsAny<CancellationToken>())).Callback<Object, CancellationToken>((document, _) => storedId = ((StringDocument) document).Id).Returns(Task.CompletedTask);

        var created = await handler.Create(entity, session.Object, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(storedId, Is.EqualTo("surlatables/abc"));
            Assert.That(created.Identifier, Is.EqualTo("surlatables/abc"));
        }
    }

    [Test]
    public async Task Create_WithUnsetStringIdentifier_LetsSessionAssignIdentifier()
    {
        var session = new Mock<IAsyncDocumentSession>();
        var handler = new CreationHandler<StringDocument, String, IAsyncDocumentSession>();
        var entity = new StringDocument();
        String? storedId = "unexpected";

        session.Setup(t => t.StoreAsync(It.IsAny<Object>(), It.IsAny<CancellationToken>())).Callback<Object, CancellationToken>((document, _) => AssignStringId((StringDocument) document, out storedId)).Returns(Task.CompletedTask);

        var created = await handler.Create(entity, session.Object, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(storedId, Is.Null);
            Assert.That(created.Identifier, Is.EqualTo("surlatables/1-A"));
        }
    }

    [Test]
    public async Task Create_WithUnsetValueTypeIdentifier_LetsSessionAssignIdentifier()
    {
        var session = new Mock<IAsyncDocumentSession>();
        var handler = new CreationHandler<Int32Document, Int32, IAsyncDocumentSession>();
        var entity = new Int32Document();
        var storedId = -1;

        session.Setup(t => t.StoreAsync(It.IsAny<Object>(), It.IsAny<CancellationToken>())).Callback<Object, CancellationToken>((document, _) => AssignInt32Id((Int32Document) document, out storedId)).Returns(Task.CompletedTask);

        var created = await handler.Create(entity, session.Object, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(storedId, Is.Zero);
            Assert.That(created.Identifier, Is.EqualTo(7));
        }
    }

    [Test]
    public async Task Create_WhenSessionLeavesIdentifierUnset_EvictsDocumentAndThrowsCreationException()
    {
        var advanced = new Mock<IAsyncAdvancedSessionOperations>();
        var session = new Mock<IAsyncDocumentSession>();
        var handler = new CreationHandler<Int32Document, Int32, IAsyncDocumentSession>();
        var entity = new Int32Document();

        session.Setup(t => t.Advanced).Returns(advanced.Object);
        session.Setup(t => t.StoreAsync(It.IsAny<Object>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await Assert.ThrowsAsync<CreationException<Int32Document>>(async () => await handler.Create(entity, session.Object, CancellationToken.None));

        advanced.Verify(t => t.Evict(entity), Times.Once);
    }

    private static void AssignInt32Id(Int32Document document, out Int32 storedId)
    {
        storedId = document.Id;
        document.Id = 7;
    }

    private static void AssignStringId(StringDocument document, out String? storedId)
    {
        storedId = document.Id;
        document.Id = "surlatables/1-A";
    }

    public record Int32Document : DomainEntityBase<Int32>, ICreationMoment
    {
        public DateTimeOffset CreationMoment { get; set; }

        public Int32 Id { get; set; }
    }

    public record StringDocument : DomainEntityBase<String>, ICreationMoment
    {
        public DateTimeOffset CreationMoment { get; set; }

        public String? Id { get; set; }
    }
}
