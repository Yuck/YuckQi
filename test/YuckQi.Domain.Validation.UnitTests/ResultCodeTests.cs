using System;
using NUnit.Framework;

namespace YuckQi.Domain.Validation.UnitTests;

public class ResultCodeTests
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void ResultCode_WithSameCode_HasSameHashCode()
    {
        var resultCodeA = new ResultCode();
        var hashCodeA = resultCodeA.GetHashCode();
        var resultCodeB = new ResultCode(resultCodeA);
        var hashCodeB = resultCodeB.GetHashCode();

        Assert.That(hashCodeA, Is.EqualTo(hashCodeB));
    }

    [Test]
    public void ResultCode_StringConversion_IsAsExpected()
    {
        var resultCode = new ResultCode("test");

        Assert.That(resultCode.ToString(), Is.EquivalentTo("test"));
    }

    [Test]
    public void ResultCode_Equality_IsAsExpected()
    {
        var resultCodeA = new ResultCode("test");
        var resultCodeB = new ResultCode("test");

        Assert.That(resultCodeA.Equals("test"), Is.True);
        Assert.That(resultCodeA.Equals(resultCodeB), Is.True);
        Assert.That(resultCodeA.Equals(1234), Is.False);
    }

    [Test]
    public void ResultCode_ToString_IsAsExpected()
    {
        var resultCode = new ResultCode("test");

        Assert.That(resultCode.ToString(), Is.EqualTo("test"));
    }

    [Test]
    public void ResultCode_ImplicitConversionFromString_CreatesResultCode()
    {
        ResultCode code = "my-code";

        Assert.That((String) code, Is.EqualTo("my-code"));
    }

    [Test]
    public void ResultCode_StaticFields_HaveExpectedValues()
    {
        Assert.That((String) ResultCode.InvalidRequestDetail, Is.EqualTo(nameof(ResultCode.InvalidRequestDetail)));
        Assert.That((String) ResultCode.NotFound, Is.EqualTo(nameof(ResultCode.NotFound)));
    }

    [Test]
    public void ResultCode_WithExpression_CopiesValue()
    {
        var original = new ResultCode("test");
        var copy = original with { };

        Assert.That(copy, Is.EqualTo(original));
        Assert.That(copy.GetHashCode(), Is.EqualTo(original.GetHashCode()));
    }
}
