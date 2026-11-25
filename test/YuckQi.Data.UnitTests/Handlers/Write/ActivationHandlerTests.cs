using NUnit.Framework;
using YuckQi.Data.Handlers.Write;
using YuckQi.Data.Handlers.Write.Abstract.Interfaces;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract;

namespace YuckQi.Data.UnitTests.Handlers.Write;

public class ActivationHandlerTests
{
    [Test]
    public void Activate_WhenNotActive_SetsActivationMoment()
    {
        var reviser = new StubRevisionHandler();
        var handler = new ActivationHandler<SurLaTable, Int32, Object>(reviser);
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        var result = handler.Activate(entity, new Object());

        Assert.That(result.ActivationMoment, Is.Not.Null);
    }

    [Test]
    public void Activate_WhenAlreadyActive_DoesNotChangeActivationMoment()
    {
        var reviser = new StubRevisionHandler();
        var handler = new ActivationHandler<SurLaTable, Int32, Object>(reviser);
        var existing = DateTimeOffset.UtcNow.AddDays(-1);
        var entity = new SurLaTable { Identifier = 1, Name = "ABC", ActivationMoment = existing };

        var result = handler.Activate(entity, new Object());

        Assert.That(result.ActivationMoment, Is.EqualTo(existing));
    }

    [Test]
    public void Deactivate_WhenActive_ClearsActivationMoment()
    {
        var reviser = new StubRevisionHandler();
        var handler = new ActivationHandler<SurLaTable, Int32, Object>(reviser);
        var entity = new SurLaTable { Identifier = 1, Name = "ABC", ActivationMoment = DateTimeOffset.UtcNow };

        var result = handler.Deactivate(entity, new Object());

        Assert.That(result.ActivationMoment, Is.Null);
    }

    [Test]
    public void Deactivate_WhenNotActive_DoesNotChangeActivationMoment()
    {
        var reviser = new StubRevisionHandler();
        var handler = new ActivationHandler<SurLaTable, Int32, Object>(reviser);
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        var result = handler.Deactivate(entity, new Object());

        Assert.That(result.ActivationMoment, Is.Null);
    }

    [Test]
    public void Activate_WhenRevisionFails_RestoresActivationMomentAndCanBeRetried()
    {
        var reviser = new FailingOnceRevisionHandler();
        var handler = new ActivationHandler<SurLaTable, Int32, Object>(reviser);
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        Assert.Throws<InvalidOperationException>(() => handler.Activate(entity, new Object()));
        Assert.That(entity.ActivationMoment, Is.Null);

        var result = handler.Activate(entity, new Object());

        Assert.That(result.ActivationMoment, Is.Not.Null);
        Assert.That(reviser.Calls, Is.EqualTo(2));
    }

    [Test]
    public async Task Activate_Async_WhenRevisionFails_RestoresActivationMomentAndCanBeRetried()
    {
        var reviser = new FailingOnceRevisionHandler();
        var handler = new ActivationHandler<SurLaTable, Int32, Object>(reviser);
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Activate(entity, new Object(), CancellationToken.None));
        Assert.That(entity.ActivationMoment, Is.Null);

        var result = await handler.Activate(entity, new Object(), CancellationToken.None);

        Assert.That(result.ActivationMoment, Is.Not.Null);
        Assert.That(reviser.Calls, Is.EqualTo(2));
    }

    [Test]
    public void Deactivate_WhenRevisionFails_RestoresActivationMomentAndCanBeRetried()
    {
        var reviser = new FailingOnceRevisionHandler();
        var handler = new ActivationHandler<SurLaTable, Int32, Object>(reviser);
        var existing = DateTimeOffset.UtcNow.AddDays(-1);
        var entity = new SurLaTable { Identifier = 1, Name = "ABC", ActivationMoment = existing };

        Assert.Throws<InvalidOperationException>(() => handler.Deactivate(entity, new Object()));
        Assert.That(entity.ActivationMoment, Is.EqualTo(existing));

        var result = handler.Deactivate(entity, new Object());

        Assert.That(result.ActivationMoment, Is.Null);
        Assert.That(reviser.Calls, Is.EqualTo(2));
    }

    [Test]
    public async Task Deactivate_Async_WhenRevisionFails_RestoresActivationMomentAndCanBeRetried()
    {
        var reviser = new FailingOnceRevisionHandler();
        var handler = new ActivationHandler<SurLaTable, Int32, Object>(reviser);
        var existing = DateTimeOffset.UtcNow.AddDays(-1);
        var entity = new SurLaTable { Identifier = 1, Name = "ABC", ActivationMoment = existing };

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Deactivate(entity, new Object(), CancellationToken.None));
        Assert.That(entity.ActivationMoment, Is.EqualTo(existing));

        var result = await handler.Deactivate(entity, new Object(), CancellationToken.None);

        Assert.That(result.ActivationMoment, Is.Null);
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

    public record SurLaTable : DomainEntityBase<Int32>, IActivationMoment, IRevisionMoment
    {
        public DateTimeOffset? ActivationMoment { get; set; }

        public String Name { get; set; } = String.Empty;

        public DateTimeOffset RevisionMoment { get; set; }
    }
}
