using System.Collections.Concurrent;
using NUnit.Framework;
using YuckQi.Data.Exceptions;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Data.MemDb.Handlers.Write;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract;

namespace YuckQi.Data.MemDb.UnitTests.Handlers.Write;

public class RevisionHandlerTests
{
    [Test]
    public void Revise_WithAutoPropertyHandling_SetsRevisionMoment()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>(new Dictionary<Int32, SurLaTable> { { 1, new SurLaTable { Identifier = 1, Name = "ABC" } } });
        var reviser = new RevisionHandler<SurLaTable, Int32, Object>(entities, new RevisionOptions(PropertyHandling.Auto));
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        var revised = reviser.Revise(entity, scope);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entity.Identifier, Is.EqualTo(revised.Identifier));
            Assert.That(entity.RevisionMoment.UtcDateTime, Is.GreaterThan(DateTime.MinValue));
            Assert.That(entities.Values.ToList(), Does.Contain(revised));
        }
    }

    [Test]
    public void Revise_WithManualPropertyHandling_PreservesRevisionMoment()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>(new Dictionary<Int32, SurLaTable> { { 1, new SurLaTable { Identifier = 1, Name = "ABC" } } });
        var reviser = new RevisionHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var revisionMomentUtc = DateTime.UtcNow;
        var entity = new SurLaTable { Identifier = 1, Name = "ABC", RevisionMoment = revisionMomentUtc };

        var revised = reviser.Revise(entity, scope);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entity.Identifier, Is.EqualTo(revised.Identifier));
            Assert.That(entity.RevisionMoment.UtcDateTime, Is.EqualTo(revisionMomentUtc));
            Assert.That(entities.Values.ToList(), Does.Contain(revised));
        }
    }

    [Test]
    public void Revise_WhenEntityNotFound_ThrowsRevisionException()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>(new Dictionary<Int32, SurLaTable> { { 1, new SurLaTable { Identifier = 1, Name = "ABC" } } });
        var reviser = new RevisionHandler<SurLaTable, Int32, Object>(entities, new RevisionOptions(PropertyHandling.Auto));
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 2, Name = "ABC" };

        Assert.That(() => reviser.Revise(entity, scope), Throws.TypeOf<RevisionException<SurLaTable, Int32>>());
    }

    [Test]
    public async Task Revise_Async_UpdatesEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>(new Dictionary<Int32, SurLaTable> { { 1, new SurLaTable { Identifier = 1, Name = "ABC" } } });
        var reviser = new RevisionHandler<SurLaTable, Int32, Object>(entities, new RevisionOptions(PropertyHandling.Auto));
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "DEF" };

        var revised = await reviser.Revise(entity, scope, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(revised.Identifier, Is.EqualTo(1));
            Assert.That(entities.Values.ToList(), Does.Contain(revised));
        }
    }

    [Test]
    public void Revise_NullIdentifier_ThrowsInvalidOperationException()
    {
        var entities = new ConcurrentDictionary<String, StringEntity>();
        var reviser = new RevisionHandler<StringEntity, String, Object>(entities, new RevisionOptions(PropertyHandling.Auto));
        var scope = new Object();
        var entity = new StringEntity { Identifier = null! };

        Assert.Throws<InvalidOperationException>(() => reviser.Revise(entity, scope));
    }

    public record StringEntity : DomainEntityBase<String>, IRevisionMoment
    {
        public String Name { get; set; } = String.Empty;

        public DateTimeOffset RevisionMoment { get; set; }
    }

    public record SurLaTable : DomainEntityBase<Int32>, IRevisionMoment
    {
        public String Name { get; set; } = String.Empty;

        public DateTimeOffset RevisionMoment { get; set; }
    }
}
