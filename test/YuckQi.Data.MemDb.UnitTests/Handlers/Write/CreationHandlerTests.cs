using System.Collections.Concurrent;
using NUnit.Framework;
using YuckQi.Data.Exceptions;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Data.MemDb.Handlers.Write;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract;

namespace YuckQi.Data.MemDb.UnitTests.Handlers.Write;

public class CreationHandlerTests
{
    [Test]
    public void Create_WithValidEntity_ReturnsCreatedEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        var created = creator.Create(entity, scope);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entities.Values.ToList(), Does.Contain(created));
            Assert.That(entity.Identifier, Is.EqualTo(created.Identifier));
        }
    }

    [Test]
    public void Create_WhenDuplicate_ThrowsCreationException()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        creator.Create(entity, scope);

        Assert.That(() => creator.Create(entity, scope), Throws.Exception.TypeOf<CreationException<SurLaTable>>());
    }

    [Test]
    public async Task Create_Async_ReturnsEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 1, Name = "ABC" };

        var created = await creator.Create(entity, scope, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entities.Values.ToList(), Does.Contain(created));
            Assert.That(entity.Identifier, Is.EqualTo(created.Identifier));
        }
    }

    [Test]
    public void Create_NullIdentifier_ThrowsInvalidOperationException()
    {
        var entities = new ConcurrentDictionary<String, StringEntity>();
        var creator = new CreationHandler<StringEntity, String, Object>(entities);
        var scope = new Object();
        var entity = new StringEntity { Identifier = null! };

        Assert.Throws<InvalidOperationException>(() => creator.Create(entity, scope));
    }

    [Test]
    public void Create_UnsetValueTypeIdentifier_ThrowsInvalidOperationException()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities);
        var scope = new Object();
        var entity = new SurLaTable { Name = "ABC" };

        using (Assert.EnterMultipleScope())
        {
            Assert.Throws<InvalidOperationException>(() => creator.Create(entity, scope));
            Assert.That(entities, Is.Empty);
        }
    }

    [Test]
    public void Create_ZeroIdentifierWithAllowDefaultIdentifier_StoresEntity()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities, new CreationOptions<Int32>(allowDefaultIdentifier: true));
        var scope = new Object();
        var entity = new SurLaTable { Identifier = 0, Name = "Unknown" };

        var created = creator.Create(entity, scope);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(created.Identifier, Is.Zero);
            Assert.That(entities[0], Is.SameAs(entity));
        }
    }

    [Test]
    public void Create_UnsetValueTypeIdentifierWithFactory_UsesFactoryIdentifier()
    {
        var entities = new ConcurrentDictionary<Int32, SurLaTable>();
        var creator = new CreationHandler<SurLaTable, Int32, Object>(entities, new CreationOptions<Int32>(() => 5));
        var scope = new Object();
        var entity = new SurLaTable { Name = "ABC" };

        var created = creator.Create(entity, scope);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(created.Identifier, Is.EqualTo(5));
            Assert.That(entities.Keys, Is.EqualTo(new[] { 5 }));
        }
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
