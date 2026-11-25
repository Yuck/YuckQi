using NUnit.Framework;
using YuckQi.Domain.Entities.Abstract;
using YuckQi.Domain.Entities.Extensions;

namespace YuckQi.Domain.UnitTests.Entities.Abstract;

public class DomainEntityBaseTests
{
    private record Entity : DomainEntityBase<Guid>;

    private record NamedEntity : DomainEntityBase<Guid>
    {
        public String Name { get; set; } = String.Empty;
    }

    private record StringIdEntity : DomainEntityBase<String>;

    [Test]
    public void EntityBase_GetIdentifier_HasExpectedValue()
    {
        var identifier = Guid.NewGuid();
        var entity = new Entity { Identifier = identifier };

        Assert.That(entity.Identifier, Is.EqualTo(identifier));
    }

    [Test]
    public void EntityBase_WhenIdentifierNotSet_HasDefaultValue()
    {
        var entity = new Entity();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entity.Identifier, Is.EqualTo(default(Guid)));
            Assert.That(entity.Identifier.IsSet(), Is.False);
        }
    }

    [Test]
    public void EntityBase_WhenStringIdentifierNotSet_IsNull()
    {
        var entity = new StringIdEntity();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entity.Identifier, Is.Null);
            Assert.That(entity.Identifier.IsSet(), Is.False);
        }
    }

    [Test]
    public void EntityBase_WithDifferentIdentifiers_AreNotEqual()
    {
        var first = new NamedEntity { Identifier = Guid.NewGuid(), Name = "ABC" };
        var second = first with { Identifier = Guid.NewGuid() };

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public void EntityBase_WithSameIdentifierAndDifferentValues_AreNotEqual()
    {
        var first = new NamedEntity { Identifier = Guid.NewGuid(), Name = "ABC" };
        var second = first with { Name = "DEF" };

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public void EntityBase_WithSameValues_AreEqual()
    {
        var first = new NamedEntity { Name = "ABC" };
        var second = new NamedEntity { Name = "ABC" };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(first, Is.EqualTo(second));
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        }
    }

    [Test]
    public void EntityBase_WithExpression_CopiesIdentifier()
    {
        var entity = new Entity { Identifier = Guid.NewGuid() };
        var copy = entity with { };

        Assert.That(copy.Identifier, Is.EqualTo(entity.Identifier));
    }

    [Test]
    public void EntityBase_WhenIdentifierAssignedAfterInit_HasExpectedValue()
    {
        var identifier = Guid.NewGuid();
        var entity = new Entity();

        entity.Identifier = identifier;

        Assert.That(entity.Identifier, Is.EqualTo(identifier));
    }
}
