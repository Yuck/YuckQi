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

    [Test]
    public void Delete_WhenRevisionFails_RestoresDeletionMomentAndCanBeRetried()
    {
        var reviser = new FailingOnceRevisionHandler();
        var handler = new LogicalDeletionHandler<SurLaTable, Int32, Object>(reviser);
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        Assert.Throws<InvalidOperationException>(() => handler.Delete(entity, new Object()));
        Assert.That(entity.DeletionMoment, Is.Null);

        var result = handler.Delete(entity, new Object());

        Assert.That(result.DeletionMoment, Is.Not.Null);
        Assert.That(reviser.Calls, Is.EqualTo(2));
    }

    [Test]
    public async Task Delete_Async_WhenRevisionFails_RestoresDeletionMomentAndCanBeRetried()
    {
        var reviser = new FailingOnceRevisionHandler();
        var handler = new LogicalDeletionHandler<SurLaTable, Int32, Object>(reviser);
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Delete(entity, new Object(), CancellationToken.None));
        Assert.That(entity.DeletionMoment, Is.Null);

        var result = await handler.Delete(entity, new Object(), CancellationToken.None);

        Assert.That(result.DeletionMoment, Is.Not.Null);
        Assert.That(reviser.Calls, Is.EqualTo(2));
    }

    [Test]
    public void Restore_WhenRevisionFails_RestoresDeletionMomentAndCanBeRetried()
    {
        var reviser = new FailingOnceRevisionHandler();
        var handler = new LogicalDeletionHandler<SurLaTable, Int32, Object>(reviser);
        var existing = DateTimeOffset.UtcNow.AddDays(-1);
        var entity = new SurLaTable { Identifier = 1, Name = "ABC", DeletionMoment = existing };

        Assert.Throws<InvalidOperationException>(() => handler.Restore(entity, new Object()));
        Assert.That(entity.DeletionMoment, Is.EqualTo(existing));

        var result = handler.Restore(entity, new Object());

        Assert.That(result.DeletionMoment, Is.Null);
        Assert.That(reviser.Calls, Is.EqualTo(2));
    }

    [Test]
    public async Task Restore_Async_WhenRevisionFails_RestoresDeletionMomentAndCanBeRetried()
    {
        var reviser = new FailingOnceRevisionHandler();
        var handler = new LogicalDeletionHandler<SurLaTable, Int32, Object>(reviser);
        var existing = DateTimeOffset.UtcNow.AddDays(-1);
        var entity = new SurLaTable { Identifier = 1, Name = "ABC", DeletionMoment = existing };

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Restore(entity, new Object(), CancellationToken.None));
        Assert.That(entity.DeletionMoment, Is.EqualTo(existing));

        var result = await handler.Restore(entity, new Object(), CancellationToken.None);

        Assert.That(result.DeletionMoment, Is.Null);
        Assert.That(reviser.Calls, Is.EqualTo(2));
    }

    private class FailingOnceRevisionHandler : IRevisionHandler<SurLaTable, Int32, Object?>
    {
        public Int32 Calls { get; private set; }

        public SurLaTable Revise(SurLaTable entity, Object? scope)
        {
            Calls++;

            if (Calls == 1)
                throw new InvalidOperationException();

            return entity;
        }

        public IEnumerable<SurLaTable> Revise(IEnumerable<SurLaTable> entities, Object? scope) => entities.Select(entity => Revise(entity, scope)).ToList();

        public async Task<SurLaTable> Revise(SurLaTable entity, Object? scope, CancellationToken cancellationToken)
        {
            await Task.Yield();

            return Revise(entity, scope);
        }

        public async Task<IEnumerable<SurLaTable>> Revise(IEnumerable<SurLaTable> entities, Object? scope, CancellationToken cancellationToken)
        {
            await Task.Yield();

            return Revise(entities, scope);
        }
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
