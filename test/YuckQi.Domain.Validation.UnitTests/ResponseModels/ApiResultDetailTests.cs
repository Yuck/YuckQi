using System.Text.Json;
using NUnit.Framework;
using YuckQi.Domain.Validation.JsonConverters;
using YuckQi.Domain.Validation.ResponseModels;

namespace YuckQi.Domain.Validation.UnitTests.ResponseModels;

public class ApiResultDetailTests
{
    [Test]
    public void ResultDetail_CanBeDeserialized_ToApiResultDetail()
    {
        var message = "a test message";
        var code = "a test code";
        var property = "property";
        var type = ResultType.Warning;
        var detail = new ResultDetail(message, code, property, type);
        var options = new JsonSerializerOptions { Converters = { new ResultCodeJsonConverter(), new ResultMessageJsonConverter() } };
        var json = JsonSerializer.Serialize(detail, options);
        var response = JsonSerializer.Deserialize<ApiResultDetail>(json, options);

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Message, Is.EqualTo(message));
        Assert.That(response!.Code, Is.EqualTo(code));
        Assert.That(response!.Property, Is.EqualTo(property));
        Assert.That(response!.Type, Is.EqualTo(type));
    }

    [Test]
    public void ApiResultDetail_SetProperties_HaveMatchingValues()
    {
        var detail = new ApiResultDetail
        {
            Code = "test-code",
            Message = "test message",
            Property = "Name",
            Type = ResultType.Error
        };

        Assert.That(detail.Code, Is.EqualTo("test-code"));
        Assert.That(detail.Message, Is.EqualTo("test message"));
        Assert.That(detail.Property, Is.EqualTo("Name"));
        Assert.That(detail.Type, Is.EqualTo(ResultType.Error));
    }

    [Test]
    public void ApiResultDetail_DefaultInstance_HasNullProperties()
    {
        var detail = new ApiResultDetail();

        Assert.That(detail.Code, Is.Null);
        Assert.That(detail.Message, Is.Null);
        Assert.That(detail.Property, Is.Null);
        Assert.That(detail.Type, Is.EqualTo(ResultType.Unknown));
    }

    [Test]
    public void ApiResultDetail_WithExpression_CopiesValues()
    {
        var original = new ApiResultDetail
        {
            Code = "original-code",
            Message = "original message",
            Property = "Name",
            Type = ResultType.Warning
        };
        var copy = original with { Message = "updated message" };

        Assert.That(copy.Code, Is.EqualTo("original-code"));
        Assert.That(copy.Message, Is.EqualTo("updated message"));
        Assert.That(copy.Property, Is.EqualTo("Name"));
        Assert.That(copy.Type, Is.EqualTo(ResultType.Warning));
    }
}
