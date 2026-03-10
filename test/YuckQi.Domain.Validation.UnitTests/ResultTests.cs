using NUnit.Framework;
using YuckQi.Domain.Validation;

namespace YuckQi.Domain.Validation.UnitTests;

public class ResultTests
{
    [Test]
    public void Result_NotFound_HasNotFoundResultCode()
    {
        var detail = new List<ResultDetail> { ResultDetail.NotFound<String, Int32>(1) }.AsReadOnly();
        var result = new Result<String>(detail);

        Assert.That(result.HasResultCode(ResultCode.NotFound), Is.True);
    }

    [Test]
    public void Result_WithErrors_IsNotValid()
    {
        var detail = new List<ResultDetail> { new ("test") };
        var result = new Result<String>("test", detail);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Result_WithNoDetail_IsValid()
    {
        var result = new Result<String>("test");

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Result_WithOnlyWarnings_IsValid()
    {
        var detail = new List<ResultDetail> { new (new ResultMessage("test"), type: ResultType.Warning) };
        var result = new Result<String>("test", detail);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Result_Content_IsValid()
    {
        var content = Guid.NewGuid().ToString();
        var result = new Result<String>(content);

        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Content, Is.SameAs(content));
    }

    [Test]
    public void Result_NotFound_IsValid()
    {
        var identifier = Guid.NewGuid().ToString();
        var result = Result<String>.NotFound(identifier);
        var errors = result.Detail.Where(t => t.Code == ResultCode.NotFound);

        Assert.That(errors.Count(), Is.EqualTo(1));
    }

    [Test]
    public void Result_WithNullDetail_HasEmptyDetail()
    {
        var result = new Result(null);

        Assert.That(result.Detail, Is.Not.Null);
        Assert.That(result.Detail, Is.Empty);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Result_HasResultCode_ReturnsFalse_WhenCodeNotPresent()
    {
        var result = new Result<String>("test");

        Assert.That(result.HasResultCode(ResultCode.NotFound), Is.False);
    }

    [Test]
    public void Result_NotFound_WithCustomMessage_HasMessage()
    {
        var result = Result<String>.NotFound("abc", "custom message");

        Assert.That((String) result.Detail.First().Message, Is.EqualTo("custom message"));
    }

    [Test]
    public void Result_WithExpression_CopiesDetail()
    {
        var detail = new List<ResultDetail> { new ("test") };
        var original = new Result<String>("content", detail);
        var copy = original with { };

        Assert.That(copy.Detail, Is.EqualTo(original.Detail));
        Assert.That(copy.Content, Is.EqualTo(original.Content));
    }
}
