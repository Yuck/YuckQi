using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;
using YuckQi.Domain.Validation.Extensions;

namespace YuckQi.Domain.Validation.UnitTests.JsonConverters;

public class ResultDetailJsonConverterTests
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions().AddResultJsonConverters();

    [Test]
    public void ResultDetail_CanSerialize_ToJson()
    {
        var detail = new ResultDetail("Name is required", ResultCode.InvalidRequestDetail, "Name");
        var json = JsonSerializer.Serialize(detail, _options);

        Assert.That(json, Is.EqualTo("{\"Code\":\"InvalidRequestDetail\",\"Message\":\"Name is required\",\"Property\":\"Name\",\"Type\":2}"));
    }

    [Test]
    public void ResultDetail_RoundTrip_IsEqual()
    {
        var detail = new ResultDetail("msg", "code", "prop", ResultType.Warning);
        var json = JsonSerializer.Serialize(detail, _options);
        var copy = JsonSerializer.Deserialize<ResultDetail>(json, _options);

        Assert.That(copy, Is.EqualTo(detail));
    }

    [Test]
    public void ResultDetail_RoundTripWithoutCode_HasNullCode()
    {
        var detail = new ResultDetail("msg");
        var json = JsonSerializer.Serialize(detail, _options);
        var copy = JsonSerializer.Deserialize<ResultDetail>(json, _options);

        Assert.That(json, Does.Contain("\"Code\":null"));
        Assert.That(copy, Is.EqualTo(detail));
        Assert.That(copy?.Code, Is.Null);
    }

    [Test]
    public void ResultDetail_DeserializeWithoutMessage_HasEmptyMessage()
    {
        var detail = JsonSerializer.Deserialize<ResultDetail>("{\"Code\":\"code\"}", _options);

        Assert.That(detail, Is.Not.Null);
        Assert.That((String?) detail?.Message, Is.Empty);
        Assert.That(detail?.Code, Is.EqualTo(new ResultCode("code")));
        Assert.That(detail?.Type, Is.EqualTo(ResultType.Unknown));
    }

    [Test]
    public void ResultDetail_DeserializeNull_IsNull()
    {
        var detail = JsonSerializer.Deserialize<ResultDetail>("null", _options);

        Assert.That(detail, Is.Null);
    }

    [Test]
    public void ResultDetail_SerializeWithSerializerOptions_HonorsNamingPolicyAndIgnoreCondition()
    {
        var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }.AddResultJsonConverters();
        var detail = new ResultDetail("msg");
        var json = JsonSerializer.Serialize(detail, options);
        var copy = JsonSerializer.Deserialize<ResultDetail>(json, options);

        Assert.That(json, Is.EqualTo("{\"message\":\"msg\",\"type\":2}"));
        Assert.That(copy, Is.EqualTo(detail));
    }
}
