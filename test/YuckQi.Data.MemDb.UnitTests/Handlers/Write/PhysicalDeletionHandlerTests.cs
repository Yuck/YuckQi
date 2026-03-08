using System.Collections.Concurrent;
using NUnit.Framework;
using YuckQi.Data.Exceptions;
using YuckQi.Data.MemDb.Handlers.Write;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract;

namespace YuckQi.Data.MemDb.UnitTests.Handlers.Write;

public class PhysicalDeletionHandlerTests
{
    [Test]
    public void Delete_WhenEntityExists_RemovesEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var deleter = new PhysicalDeletionHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        var created = creator.Create(entity, scope);

        Assert.That(entities.Values.ToList(), Does.Contain(created));

        var deleted = deleter.Delete(created, scope);

        Assert.That(entities.Values.ToList(), Does.Not.Contain(deleted));
    }

    [Test]
    public void Delete_WhenEntityNotFound_ThrowsPhysicalDeletionException()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var deleter = new PhysicalDeletionHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        Assert.Throws<PhysicalDeletionException<SurLaTable, Int32>>(() => deleter.Delete(entity, scope));
    }

    [Test]
    public async Task Delete_Async_RemovesEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var deleter = new PhysicalDeletionHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        var created = creator.Create(entity, scope);
        var deleted = await deleter.Delete(created, scope, CancellationToken.None);

        Assert.That(entities.Values.ToList(), Does.Not.Contain(deleted));
    }

    [Test]
    public void Delete_NullIdentifier_ThrowsInvalidOperationException()
    {
        var entities = new ConcurrentDictionary<String, StringEntity>();
        var deleter = new PhysicalDeletionHandler<StringEntity, String, Object>(entities);
        var scope = new Object();
        var entity = new StringEntity { Identifier = null! };

        Assert.Throws<InvalidOperationException>(() => deleter.Delete(entity, scope));
    }

    public record StringEntity : DomainEntityBase<String>, ICreationMoment
    {
        public DateTimeOffset CreationMoment { get; set; }

        public String Name { get; set; } = String.Empty;
    }

    public record SurLaTable : DomainEntityBase<Int32>, ICreationMoment
    {
        public DateTimeOffset CreationMoment { get; set; }

        public String Name { get; set; } = String.Empty;
    }
}
