using System;
using NUnit.Framework;

namespace YuckQi.Domain.Validation.UnitTests;

public class ResultDetailTests
{
    [Test]
    public void ResultDetail_NotFound_HasNotFoundResultCode()
    {
        var detail = ResultDetail.NotFound<Object, Guid>(Guid.NewGuid());

        Assert.That(detail.Code, Is.EqualTo(ResultCode.NotFound));
    }

    [Test]
    public void ResultDetail_WithProperty_HasMatchingPropertyName()
    {
        var detail = new ResultDetail("thing missing", property: "property");

        Assert.That(detail.Property, Is.EqualTo("property"));
    }

    [Test]
    public void ResultDetail_Message_HasMatchingValue()
    {
        var message = "this is a problem";
        var detail = new ResultDetail(message);

        Assert.That((String) detail.Message, Is.EqualTo(message));
    }

    [Test]
    public void ResultDetail_DefaultType_IsError()
    {
        var detail = new ResultDetail("test");

        Assert.That(detail.Type, Is.EqualTo(ResultType.Error));
    }

    [Test]
    public void ResultDetail_NotFound_WithCustomMessage_UsesCustomMessage()
    {
        var detail = ResultDetail.NotFound<Object, Int32>(42, "custom not found");

        Assert.That((String) detail.Message, Is.EqualTo("custom not found"));
        Assert.That(detail.Code, Is.EqualTo(ResultCode.NotFound));
    }

    [Test]
    public void ResultDetail_WithExpression_CopiesValues()
    {
        var original = new ResultDetail("msg", "code", "prop", ResultType.Warning);
        var copy = original with { };

        Assert.That(copy, Is.EqualTo(original));
        Assert.That((String) copy.Message, Is.EqualTo((String) original.Message));
        Assert.That(copy.Code, Is.EqualTo(original.Code));
    }

    [Test]
    public void ResultDetail_ConstructedWithResultMessage_HasMatchingMessage()
    {
        var message = new ResultMessage("explicit message");
        var detail = new ResultDetail(message, type: ResultType.Warning);

        Assert.That((String) detail.Message, Is.EqualTo("explicit message"));
        Assert.That(detail.Code, Is.Null);
        Assert.That(detail.Property, Is.Null);
        Assert.That(detail.Type, Is.EqualTo(ResultType.Warning));
    }
}
