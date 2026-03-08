using NUnit.Framework;
using YuckQi.Data.DocumentDb.RavenDb.Attributes;

namespace YuckQi.Data.DocumentDb.RavenDb.UnitTests.Attributes;

public class DatabaseAttributeTests
{
    [Test]
    public void Constructor_SetsName()
    {
        var attribute = new DatabaseAttribute("TestDatabase");

        Assert.That(attribute.Name, Is.EqualTo("TestDatabase"));
    }

    [Test]
    public void Name_WithEmptyString_ReturnsEmptyString()
    {
        var attribute = new DatabaseAttribute(String.Empty);

        Assert.That(attribute.Name, Is.EqualTo(String.Empty));
    }
}
