using NUnit.Framework;

namespace YuckQi.Domain.Validation.UnitTests;

public class ResultMessageTests
{
    [Test]
    public void ResultMessage_ToString_MatchesOriginalMessage()
    {
        var message = new ResultMessage("this is a test");

        Assert.That(message.ToString(), Is.EqualTo("this is a test"));
    }

    [Test]
    public void ResultMessage_WithNoMessage_IsNotNull()
    {
        var message = new ResultMessage();

        Assert.That(message.ToString(), Is.Not.Null);
    }

    [Test]
    public void ResultMessage_ImplicitConversionFromString_CreatesResultMessage()
    {
        ResultMessage message = "hello";

        Assert.That((String) message, Is.EqualTo("hello"));
    }

    [Test]
    public void ResultMessage_ImplicitConversionToString_ReturnsValue()
    {
        var message = new ResultMessage("world");
        String value = message;

        Assert.That(value, Is.EqualTo("world"));
    }

    [Test]
    public void ResultMessage_NotFound_DefaultMessage_ContainsTypeName()
    {
        var message = ResultMessage.NotFound<String, Int32>(42);

        Assert.That((String) message, Does.Contain("String"));
        Assert.That((String) message, Does.Contain("42"));
    }

    [Test]
    public void ResultMessage_NotFound_WithCustomMessage_UsesCustomMessage()
    {
        var message = ResultMessage.NotFound<String, Int32>(1, "custom");

        Assert.That((String) message, Is.EqualTo("custom"));
    }

    [Test]
    public void ResultMessage_WithExpression_CopiesValue()
    {
        var original = new ResultMessage("test");
        var copy = original with { };

        Assert.That(copy, Is.EqualTo(original));
        Assert.That(copy.ToString(), Is.EqualTo(original.ToString()));
    }
}
