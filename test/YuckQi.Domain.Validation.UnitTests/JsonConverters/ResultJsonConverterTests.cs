using System.Text.Json.Serialization;
using System.Text.Json;
using NUnit.Framework;
using YuckQi.Domain.Validation.Extensions;

namespace YuckQi.Domain.Validation.UnitTests.JsonConverters;

public class ResultJsonConverterTests
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions().AddResultJsonConverters();

    public record Example(Result<Int32> R);

    [Test]
    public void Result_CanSerialize_ToJson()
    {
        var result = new Result<String>("content", [new ResultDetail("msg", "code", "prop", ResultType.Warning)]);
        var json = JsonSerializer.Serialize(result, _options);

        Assert.That(json, Is.EqualTo("{\"Content\":\"content\",\"Detail\":[{\"Code\":\"code\",\"Message\":\"msg\",\"Property\":\"prop\",\"Type\":1}],\"IsValid\":true}"));
    }

    [Test]
    public void Result_RoundTrip_HasEqualContentAndDetail()
    {
        var original = new Result<String>("content", [new ResultDetail("warning", "code", "prop", ResultType.Warning)]);
        var json = JsonSerializer.Serialize(original, _options);
        var copy = JsonSerializer.Deserialize<Result<String>>(json, _options);

        Assert.That(copy, Is.Not.Null);
        Assert.That(copy?.Content, Is.EqualTo(original.Content));
        Assert.That(copy?.Detail, Is.EqualTo(original.Detail));
        Assert.That(copy?.IsValid, Is.True);
    }

    [Test]
    public void Result_RoundTripNotFound_HasNotFoundResultCode()
    {
        var original = Result<String>.NotFound(42);
        var json = JsonSerializer.Serialize(original, _options);
        var copy = JsonSerializer.Deserialize<Result<String>>(json, _options);

        Assert.That(copy, Is.Not.Null);
        Assert.That(copy?.Content, Is.Null);
        Assert.That(copy?.IsValid, Is.False);
        Assert.That(copy?.HasResultCode(ResultCode.NotFound), Is.True);
    }

    [Test]
    public void Result_DeserializeWithIsValid_ComputesIsValidFromDetail()
    {
        var json = "{\"Content\":\"content\",\"Detail\":[{\"Message\":\"msg\",\"Type\":2}],\"IsValid\":true}";
        var result = JsonSerializer.Deserialize<Result<String>>(json, _options);

        Assert.That(result, Is.Not.Null);
        Assert.That(result?.IsValid, Is.False);
    }

    [Test]
    public void Result_DeserializeWithoutDetail_HasEmptyDetail()
    {
        var result = JsonSerializer.Deserialize<Result<String>>("{\"Content\":\"content\"}", _options);

        Assert.That(result, Is.Not.Null);
        Assert.That(result?.Content, Is.EqualTo("content"));
        Assert.That(result?.Detail, Is.Empty);
    }

    [Test]
    public void Result_AsProperty_RoundTrips()
    {
        var original = new Example(new Result<Int32>(7));
        var json = JsonSerializer.Serialize(original, _options);
        var copy = JsonSerializer.Deserialize<Example>(json, _options);

        Assert.That(copy, Is.Not.Null);
        Assert.That(copy?.R.Content, Is.EqualTo(7));
        Assert.That(copy?.R.Detail, Is.Empty);
    }

    [Test]
    public void NonGenericResult_RoundTrip_HasEqualDetail()
    {
        var original = new Result([new ResultDetail("msg", "code", "prop"), new ResultDetail("warning", type: ResultType.Warning)]);
        var json = JsonSerializer.Serialize(original, _options);
        var copy = JsonSerializer.Deserialize<Result>(json, _options);

        Assert.That(copy, Is.Not.Null);
        Assert.That(copy?.Detail, Is.EqualTo(original.Detail));
        Assert.That(copy?.IsValid, Is.False);
    }

    [Test]
    public void Result_DeserializeWithNullDetailEntry_ThrowsJsonException()
    {
        var json = "{\"Content\":\"content\",\"Detail\":[null]}";

        Assert.That(() => JsonSerializer.Deserialize<Result<String>>(json, _options), Throws.TypeOf<JsonException>());
    }

    [Test]
    public void NonGenericResult_DeserializeWithNullDetailEntry_ThrowsJsonException()
    {
        var json = "{\"Detail\":[null]}";

        Assert.That(() => JsonSerializer.Deserialize<Result>(json, _options), Throws.TypeOf<JsonException>());
    }

    [Test]
    public void Result_DeserializeWithUnmappedProperty_SkipsProperty()
    {
        var json = "{\"Content\":\"content\",\"Extra\":{\"A\":[1,2]},\"Detail\":[{\"Message\":\"msg\",\"Other\":1,\"Type\":2}]}";
        var result = JsonSerializer.Deserialize<Result<String>>(json, _options);

        Assert.That(result, Is.Not.Null);
        Assert.That(result?.Content, Is.EqualTo("content"));
        Assert.That(result?.Detail, Is.EqualTo(new[] { new ResultDetail("msg") }));
    }

    [Test]
    public void Result_DeserializeWithUnmappedPropertyDisallowed_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow }.AddResultJsonConverters();
        var json = "{\"Content\":\"content\",\"Extra\":1}";

        Assert.That(() => JsonSerializer.Deserialize<Result<String>>(json, options), Throws.TypeOf<JsonException>());
    }

    [Test]
    public void Result_RoundTripWithSourceGeneratedContext_HasEqualContentAndDetail()
    {
        var options = new JsonSerializerOptions { TypeInfoResolver = ResultJsonSerializerContext.Default }.AddResultJsonConverters();
        var original = new Result<String>("content", [new ResultDetail("msg", "code", "prop", ResultType.Warning)]);
        var json = JsonSerializer.Serialize(original, options);
        var copy = JsonSerializer.Deserialize<Result<String>>(json, options);

        Assert.That(json, Is.EqualTo("{\"Content\":\"content\",\"Detail\":[{\"Code\":\"code\",\"Message\":\"msg\",\"Property\":\"prop\",\"Type\":1}],\"IsValid\":true}"));
        Assert.That(copy, Is.Not.Null);
        Assert.That(copy?.Content, Is.EqualTo(original.Content));
        Assert.That(copy?.Detail, Is.EqualTo(original.Detail));
    }

    [Test]
    public void NonGenericResult_RoundTripWithSourceGeneratedContext_HasEqualDetail()
    {
        var options = new JsonSerializerOptions { TypeInfoResolver = ResultJsonSerializerContext.Default }.AddResultJsonConverters();
        var original = new Result([new ResultDetail("msg", "code", "prop")]);
        var json = JsonSerializer.Serialize(original, options);
        var copy = JsonSerializer.Deserialize<Result>(json, options);

        Assert.That(copy, Is.Not.Null);
        Assert.That(copy?.Detail, Is.EqualTo(original.Detail));
    }

    [Test]
    public void Result_SerializeWithStringEnumConverter_WritesTypeAsString()
    {
        var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } }.AddResultJsonConverters();
        var original = new Result<String>("content", [new ResultDetail("msg", type: ResultType.Warning)]);
        var json = JsonSerializer.Serialize(original, options);
        var copy = JsonSerializer.Deserialize<Result<String>>(json, options);

        Assert.That(json, Does.Contain("\"Type\":\"Warning\""));
        Assert.That(copy?.Detail, Is.EqualTo(original.Detail));
    }
}
