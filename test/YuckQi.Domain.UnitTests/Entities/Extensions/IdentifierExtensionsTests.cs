using NUnit.Framework;
using YuckQi.Domain.Entities.Extensions;

namespace YuckQi.Domain.UnitTests.Entities.Extensions;

public class IdentifierExtensionsTests
{
    [Test]
    public void IsSet_WithDefaultGuid_IsFalse()
    {
        Assert.That(Guid.Empty.IsSet(), Is.False);
    }

    [Test]
    public void IsSet_WithDefaultInt32_IsFalse()
    {
        Assert.That(0.IsSet(), Is.False);
    }

    [Test]
    public void IsSet_WithEmptyString_IsTrue()
    {
        Assert.That(String.Empty.IsSet(), Is.True);
    }

    [Test]
    public void IsSet_WithGuid_IsTrue()
    {
        Assert.That(Guid.NewGuid().IsSet(), Is.True);
    }

    [Test]
    public void IsSet_WithInt32_IsTrue()
    {
        Assert.That(42.IsSet(), Is.True);
    }

    [Test]
    public void IsSet_WithNegativeInt32_IsTrue()
    {
        Assert.That((-1).IsSet(), Is.True);
    }

    [Test]
    public void IsSet_WithNullString_IsFalse()
    {
        String? identifier = null;

        Assert.That(identifier.IsSet(), Is.False);
    }

    [Test]
    public void IsSet_WithString_IsTrue()
    {
        Assert.That("ABC".IsSet(), Is.True);
    }

    [Test]
    public void IsSet_WithUnsetValueObject_IsFalse()
    {
        Assert.That(new Key(Guid.Empty).IsSet(), Is.False);
    }

    [Test]
    public void IsSet_WithValueObject_IsTrue()
    {
        Assert.That(new Key(Guid.NewGuid()).IsSet(), Is.True);
    }

    private readonly record struct Key(Guid Value);
}
