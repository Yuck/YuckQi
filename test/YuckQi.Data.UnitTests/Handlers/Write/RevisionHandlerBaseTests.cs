using NUnit.Framework;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract;

namespace YuckQi.Data.UnitTests.Handlers.Write;

public class RevisionHandlerBaseTests
{
    [Test]
    public void Revise_Batch_WritesEachEntityOnceWithoutEnumeration()
    {
        var handler = new CountingRevisionHandler();
        var entities = new[] { new SurLaTable { Identifier = 1 }, new SurLaTable { Identifier = 2 } };

        handler.Revise(entities, new Object());

        Assert.That(handler.Calls, Is.EqualTo(2));
    }

    [Test]
    public void Revise_Batch_WritesEachEntityOnceWhenEnumeratedRepeatedly()
    {
        var handler = new CountingRevisionHandler();
        var entities = new[] { new SurLaTable { Identifier = 1 }, new SurLaTable { Identifier = 2 } };

        var revised = handler.Revise(entities, new Object());
        var first = revised.ToList();
        var second = revised.ToList();

        Assert.That(handler.Calls, Is.EqualTo(2));
        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public async Task Revise_Async_Batch_WritesEntitiesSequentially()
    {
        var handler = new CountingRevisionHandler();
        var entities = new[] { new SurLaTable { Identifier = 1 }, new SurLaTable { Identifier = 2 }, new SurLaTable { Identifier = 3 } };

        var revised = (await handler.Revise(entities, new Object(), CancellationToken.None)).ToList();

        Assert.That(handler.Calls, Is.EqualTo(3));
        Assert.That(handler.MaximumConcurrency, Is.EqualTo(1));
        Assert.That(revised.Select(entity => entity.Identifier), Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void Revise_Batch_WhenScopeIsNull_ThrowsArgumentNullException()
    {
        var handler = new CountingRevisionHandler();
        var entities = new[] { new SurLaTable { Identifier = 1 } };

        Assert.Throws<ArgumentNullException>(() => handler.Revise(entities, null));
        Assert.That(handler.Calls, Is.Zero);
    }

    private class CountingRevisionHandler() : RevisionHandlerBase<SurLaTable, Int32, Object>
    {
        private Int32 _concurrency;

        public Int32 Calls { get; private set; }

        public Int32 MaximumConcurrency { get; private set; }

        protected override Boolean DoRevise(SurLaTable entity, Object? scope)
        {
            Calls++;

            return true;
        }

        protected override async Task<Boolean> DoRevise(SurLaTable entity, Object? scope, CancellationToken cancellationToken)
        {
            _concurrency++;
            MaximumConcurrency = Math.Max(MaximumConcurrency, _concurrency);
            Calls++;

            await Task.Delay(10, cancellationToken);

            _concurrency--;

            return true;
        }
    }

    public record SurLaTable : DomainEntityBase<Int32>, IRevisionMoment
    {
        public DateTimeOffset RevisionMoment { get; set; }
    }
}
