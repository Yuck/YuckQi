using NUnit.Framework;
using YuckQi.Data.DocumentDb.MongoDb.Attributes;

namespace YuckQi.Data.DocumentDb.MongoDb.UnitTests.Attributes;

public class CollectionAttributeTests
{
    [Test]
    public void Constructor_SetsName()
    {
        var attribute = new CollectionAttribute("TestCollection");

        Assert.That(attribute.Name, Is.EqualTo("TestCollection"));
    }

    [Test]
    public void Name_WithEmptyString_ReturnsEmptyString()
    {
        var attribute = new CollectionAttribute(String.Empty);

        Assert.That(attribute.Name, Is.EqualTo(String.Empty));
    }
}
