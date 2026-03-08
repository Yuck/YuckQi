using System;
using NUnit.Framework;
using YuckQi.Domain.Entities.Abstract;

namespace YuckQi.Domain.UnitTests.Entities.Abstract;

public class TypeEntityBaseTests
{
    private record TypeEntity : TypeEntityBase<Int32>;

    [Test]
    public void TypeEntityBase_Name_HasExpectedValue()
    {
        var entity = new TypeEntity { Identifier = 1, Name = "Test" };

        Assert.That(entity.Name, Is.EqualTo("Test"));
    }

    [Test]
    public void TypeEntityBase_ShortName_HasExpectedValue()
    {
        var entity = new TypeEntity { Identifier = 1, Name = "Test", ShortName = "T" };

        Assert.That(entity.ShortName, Is.EqualTo("T"));
    }

    [Test]
    public void TypeEntityBase_ShortName_DefaultsToNull()
    {
        var entity = new TypeEntity { Identifier = 1, Name = "Test" };

        Assert.That(entity.ShortName, Is.Null);
    }

    [Test]
    public void TypeEntityBase_WithExpression_CopiesValues()
    {
        var entity = new TypeEntity { Identifier = 1, Name = "Original", ShortName = "O" };
        var copy = entity with { Name = "Updated" };

        Assert.That(copy.Identifier, Is.EqualTo(1));
        Assert.That(copy.Name, Is.EqualTo("Updated"));
        Assert.That(copy.ShortName, Is.EqualTo("O"));
    }
}
