using NUnit.Framework;

namespace YuckQi.Domain.Validation.UnitTests;

public class ResultTests
{
    [Test]
    public void Result_NotFound_HasNotFoundResultCode()
    {
        var detail = new List<ResultDetail> { ResultDetail.NotFound<String, Int32>(1) }.AsReadOnly();
        var result = Result<String>.Failure(detail);

        Assert.That(result.HasResultCode(ResultCode.NotFound), Is.True);
    }

    [Test]
    public void Result_Failure_WithDetail_HasDefaultContentAndDetail()
    {
        var detail = new ResultDetail("test");
        var result = Result<String>.Failure(detail);

        Assert.That(result.Content, Is.Null);
        Assert.That(result.Detail, Is.EqualTo(new[] { detail }));
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Result_Failure_WithDetailCollection_HasDefaultContentAndDetail()
    {
        var detail = new List<ResultDetail> { new ("first"), new ("second") };
        var result = Result<Int32>.Failure(detail);

        Assert.That(result.Content, Is.EqualTo(0));
        Assert.That(result.Detail, Is.EqualTo(detail));
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Result_Failure_WithErrorAndWarningDetail_IsNotValid()
    {
        var detail = new List<ResultDetail> { new ("warning", type: ResultType.Warning), new ("error") };
        var result = Result<String>.Failure(detail);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Detail, Is.EqualTo(detail));
    }

    [Test]
    public void Result_Failure_WithEmptyDetail_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Result<String>.Failure([]));
    }

    [Test]
    public void Result_Failure_WithOnlyWarningDetail_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Result<String>.Failure(new ResultDetail("warning", type: ResultType.Warning)));
    }

    [Test]
    public void Result_Failure_WithNullDetail_ThrowsArgumentNullException()
    {
        var detail = default(ResultDetail);

        Assert.Throws<ArgumentNullException>(() => Result<String>.Failure(detail!));
    }

    [Test]
    public void Result_Failure_WithNullDetailCollection_ThrowsArgumentNullException()
    {
        var detail = default(IReadOnlyCollection<ResultDetail>);

        Assert.Throws<ArgumentNullException>(() => Result<String>.Failure(detail!));
    }

    [Test]
    public void Result_WithNullDetailEntry_ThrowsArgumentException()
    {
        var detail = new List<ResultDetail> { new ("test"), null! };

        Assert.Throws<ArgumentException>(() => _ = new Result(detail));
    }

    [Test]
    public void Result_WhenSourceDetailChanges_KeepsDetailAndHashCode()
    {
        var detail = new List<ResultDetail> { new ("first") };
        var result = new Result<String>("content", detail);
        var hash = result.GetHashCode();
        var set = new HashSet<Result> { result };

        detail.Add(new ResultDetail("second"));

        Assert.That(result.Detail, Has.Count.EqualTo(1));
        Assert.That(result.GetHashCode(), Is.EqualTo(hash));
        Assert.That(set.Contains(result), Is.True);
    }

    [Test]
    public void Result_Detail_CannotBeCastToSourceList()
    {
        var detail = new List<ResultDetail> { new ("first") };
        var result = new Result(detail);

        Assert.That(result.Detail, Is.Not.SameAs(detail));
        Assert.That(result.Detail as List<ResultDetail>, Is.Null);
    }

    [Test]
    public void Result_WithResultDetailContent_IsContent()
    {
        var content = new ResultDetail("content");
        var result = new Result<ResultDetail>(content);

        Assert.That(result.Content, Is.SameAs(content));
        Assert.That(result.Detail, Is.Empty);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Result_WithResultDetailCollectionContent_IsContent()
    {
        var content = new List<ResultDetail> { new ("content") }.AsReadOnly();
        var result = new Result<IReadOnlyCollection<ResultDetail>>(content);

        Assert.That(result.Content, Is.SameAs(content));
        Assert.That(result.Detail, Is.Empty);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Result_WithNullContent_HasNullContentAndEmptyDetail()
    {
        var result = new Result<String>(null);

        Assert.That(result.Content, Is.Null);
        Assert.That(result.Detail, Is.Empty);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Result_WithUnknownTypeDetail_IsNotValid()
    {
        var detail = new List<ResultDetail> { new ("test", type: ResultType.Unknown) };
        var result = new Result<String>("test", detail);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Result_WithUndefinedTypeDetail_IsNotValid()
    {
        var detail = new List<ResultDetail> { new ("test", type: (ResultType) 42) };
        var result = new Result(detail);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Result_WithWarningAndUnknownTypeDetail_IsNotValid()
    {
        var detail = new List<ResultDetail> { new ("warning", type: ResultType.Warning), new ("unknown", type: ResultType.Unknown) };
        var result = new Result(detail);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Result_WithEqualDetailInDifferentCollections_IsEqual()
    {
        var a = new Result([new ResultDetail("test", "code", "prop")]);
        var b = new Result(new List<ResultDetail> { new ("test", "code", "prop") });

        Assert.That(a, Is.EqualTo(b));
        Assert.That(a == b, Is.True);
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void Result_WithDifferentDetail_IsNotEqual()
    {
        var a = new Result([new ResultDetail("test")]);
        var b = new Result([new ResultDetail("other")]);

        Assert.That(a, Is.Not.EqualTo(b));
        Assert.That(a != b, Is.True);
    }

    [Test]
    public void Result_WithDetailInDifferentOrder_IsNotEqual()
    {
        var a = new Result([new ResultDetail("first"), new ResultDetail("second")]);
        var b = new Result([new ResultDetail("second"), new ResultDetail("first")]);

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void Result_Generic_WithEqualContentAndDetail_IsEqual()
    {
        var a = new Result<String>("content", [new ResultDetail("test", type: ResultType.Warning)]);
        var b = new Result<String>("content", new List<ResultDetail> { new ("test", type: ResultType.Warning) });

        Assert.That(a, Is.EqualTo(b));
        Assert.That(a == b, Is.True);
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void Result_Generic_WithDifferentContent_IsNotEqual()
    {
        var a = new Result<String>("content", [new ResultDetail("test")]);
        var b = new Result<String>("other", [new ResultDetail("test")]);

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void Result_Generic_WithDifferentDetail_IsNotEqual()
    {
        var a = new Result<String>("content", [new ResultDetail("test")]);
        var b = new Result<String>("content", [new ResultDetail("other")]);

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void Result_AndGenericResult_WithEqualDetail_AreNotEqual()
    {
        var a = new Result([new ResultDetail("test")]);
        var b = Result<String>.Failure(new ResultDetail("test"));

        Assert.That(a, Is.Not.EqualTo(b));
        Assert.That(b, Is.Not.EqualTo(a));
        Assert.That(a.Equals((Object) b), Is.False);
    }

    [Test]
    public void Result_GenericResults_WithDifferentContentTypes_AreNotEqual()
    {
        var a = Result<String>.Failure(new ResultDetail("test"));
        var b = Result<Int32>.Failure(new ResultDetail("test"));

        Assert.That(a.Equals((Result) b), Is.False);
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
