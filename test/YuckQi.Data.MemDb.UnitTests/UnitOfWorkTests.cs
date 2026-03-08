using System.Collections;
using System.Collections.Concurrent;
using NUnit.Framework;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract;

namespace YuckQi.Data.MemDb.UnitTests;

public class UnitOfWorkTests
{
    [Test]
    public void Constructor_WithEntities_CreatesInstance()
    {
        var entities = new Dictionary<Type, IDictionary>
        {
            { typeof(SurLaTable), new ConcurrentDictionary<Int32, SurLaTable>() }
        };
        var uow = new UnitOfWork<Object>(entities);

        Assert.That(uow, Is.Not.Null);
        Assert.That(uow.Scope, Is.Not.Null);
    }

    [Test]
    public void Constructor_WithEntitiesAndScope_CreatesInstance()
    {
        var entities = new Dictionary<Type, IDictionary>
        {
            { typeof(SurLaTable), new ConcurrentDictionary<Int32, SurLaTable>() }
        };
        var scope = new Object();
        var uow = new UnitOfWork<Object>(entities, scope);

        Assert.That(uow.Scope, Is.SameAs(scope));
    }

    [Test]
    public void Dispose_DoesNotThrow()
    {
        var entities = new Dictionary<Type, IDictionary>();
        var uow = new UnitOfWork<Object>(entities);

        Assert.DoesNotThrow(() => uow.Dispose());
    }

    [Test]
    public void SaveChanges_DoesNotThrow()
    {
        var entities = new Dictionary<Type, IDictionary>();
        var uow = new UnitOfWork<Object>(entities);

        Assert.DoesNotThrow(() => uow.SaveChanges());
    }

    [Test]
    public void GetEntities_WithValidType_ReturnsDictionary()
    {
        var dictionary = new ConcurrentDictionary<Int32, SurLaTable>();
        dictionary.TryAdd(1, new SurLaTable { Identifier = 1, Name = "ABC" });
        var entities = new Dictionary<Type, IDictionary>
        {
            { typeof(SurLaTable), dictionary }
        };
        var uow = new UnitOfWork<Object>(entities);

        var result = uow.GetEntities<SurLaTable, Int32>();

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void GetEntities_WithMissingType_ThrowsKeyNotFoundException()
    {
        var entities = new Dictionary<Type, IDictionary>();
        var uow = new UnitOfWork<Object>(entities);

        Assert.Throws<KeyNotFoundException>(() => uow.GetEntities<SurLaTable, Int32>());
    }

    [Test]
    public void GetEntities_WithInvalidCast_ThrowsInvalidCastException()
    {
        var entities = new Dictionary<Type, IDictionary>
        {
            { typeof(SurLaTable), new Hashtable() }
        };
        var uow = new UnitOfWork<Object>(entities);

        Assert.Throws<InvalidCastException>(() => uow.GetEntities<SurLaTable, Int32>());
    }

    [Test]
    public void NonGenericConstructor_WithEntities_CreatesInstance()
    {
        var entities = new Dictionary<Type, IDictionary>();
        var uow = new UnitOfWork(entities);

        Assert.That(uow, Is.Not.Null);
    }

    [Test]
    public void NonGenericConstructor_WithEntitiesAndScope_CreatesInstance()
    {
        var entities = new Dictionary<Type, IDictionary>();
        var scope = new Object();
        var uow = new UnitOfWork(entities, scope);

        Assert.That(uow.Scope, Is.SameAs(scope));
    }

    public record SurLaTable : DomainEntityBase<Int32>, ICreationMoment
    {
        public DateTimeOffset CreationMoment { get; set; }

        public String Name { get; set; } = String.Empty;
    }
}
