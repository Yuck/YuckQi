using System.Collections.Concurrent;
using NUnit.Framework;
using YuckQi.Data.Exceptions;
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
