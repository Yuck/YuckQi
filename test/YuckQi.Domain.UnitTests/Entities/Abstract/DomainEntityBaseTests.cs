using System;
using NUnit.Framework;
using YuckQi.Domain.Entities.Abstract;

namespace YuckQi.Domain.UnitTests.Entities.Abstract;

public class DomainEntityBaseTests
{
    private record Entity : DomainEntityBase<Guid>;

    [Test]
    public void EntityBase_GetIdentifier_HasExpectedValue()
    {
        var identifier = Guid.NewGuid();
        var entity = new Entity { Identifier = identifier };

        Assert.That(entity.Identifier, Is.EqualTo(identifier));
    }

    [Test]
    public void EntityBase_WithExpression_CopiesIdentifier()
    {
        var entity = new Entity { Identifier = Guid.NewGuid() };
        var copy = entity with { };

        Assert.That(copy.Identifier, Is.EqualTo(entity.Identifier));
    }
}
