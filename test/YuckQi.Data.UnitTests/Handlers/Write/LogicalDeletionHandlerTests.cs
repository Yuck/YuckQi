using NUnit.Framework;
using YuckQi.Data.Handlers.Write;
using YuckQi.Data.Handlers.Write.Abstract.Interfaces;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract;

namespace YuckQi.Data.UnitTests.Handlers.Write;

public class LogicalDeletionHandlerTests
{
    [Test]
    public void Delete_WhenNotDeleted_SetsDeletionMoment()
    {
        var reviser = new StubRevisionHandler();
        var handler = new LogicalDeletionHandler<SurLaTable, Int32, Object>(reviser);
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        var result = handler.Delete(entity, new Object());

        Assert.That(result.DeletionMoment, Is.Not.Null);
    }

    [Test]
    public void Delete_WhenAlreadyDeleted_DoesNotChangeDeletionMoment()
    {
        var reviser = new StubRevisionHandler();
        var handler = new LogicalDeletionHandler<SurLaTable, Int32, Object>(reviser);
        var existing = DateTimeOffset.UtcNow.AddDays(-1);
        var entity = new SurLaTable { Identifier = 1, Name = "ABC", DeletionMoment = existing };

        var result = handler.Delete(entity, new Object());

        Assert.That(result.DeletionMoment, Is.EqualTo(existing));
    }

    [Test]
    public void Restore_WhenDeleted_ClearsDeletionMoment()
    {
        var reviser = new StubRevisionHandler();
        var handler = new LogicalDeletionHandler<SurLaTable, Int32, Object>(reviser);
        var entity = new SurLaTable { Identifier = 1, Name = "ABC", DeletionMoment = DateTimeOffset.UtcNow };

        var result = handler.Restore(entity, new Object());

        Assert.That(result.DeletionMoment, Is.Null);
    }

    [Test]
    public void Restore_WhenNotDeleted_DoesNotChangeDeletionMoment()
    {
        var reviser = new StubRevisionHandler();
        var handler = new LogicalDeletionHandler<SurLaTable, Int32, Object>(reviser);
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        var result = handler.Restore(entity, new Object());

        Assert.That(result.DeletionMoment, Is.Null);
    }

    private class StubRevisionHandler : IRevisionHandler<SurLaTable, Int32, Object?>
    {
        public SurLaTable Revise(SurLaTable entity, Object? scope) => entity;

        public IEnumerable<SurLaTable> Revise(IEnumerable<SurLaTable> entities, Object? scope) => entities;

        public Task<SurLaTable> Revise(SurLaTable entity, Object? scope, CancellationToken cancellationToken) => Task.FromResult(entity);

        public Task<IEnumerable<SurLaTable>> Revise(IEnumerable<SurLaTable> entities, Object? scope, CancellationToken cancellationToken) => Task.FromResult(entities);
    }

    public record SurLaTable : DomainEntityBase<Int32>, IDeletionMoment, IRevisionMoment
    {
        public String Name { get; set; } = String.Empty;

        public DateTimeOffset? DeletionMoment { get; set; }

        public DateTimeOffset RevisionMoment { get; set; }
    }
}
