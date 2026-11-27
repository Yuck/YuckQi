using System.Text.Json;
using NUnit.Framework;
using YuckQi.Domain.Validation.JsonConverters;

namespace YuckQi.Domain.Validation.UnitTests.JsonConverters;

public class ResultMessageJsonConverterTests
{
    private readonly JsonSerializerOptions _options = new () { Converters = { new ResultMessageJsonConverter() } };

    public record Example(ResultMessage M);

    public record NullableExample(ResultMessage? M);

    [Test]
    public void ResultMessage_CanDeserialize_FromJson()
    {
        var value = "something is wrong";
        var json = $"{{\"M\":\"{value}\"}}";
        var example = JsonSerializer.Deserialize<Example>(json, _options);

        Assert.That(example, Is.Not.Null);
        Assert.That(example, Is.EqualTo(new Example(new ResultMessage($"{value}"))));
    }

    [Test]
    public void ResultMessage_CanSerialize_ToJson()
    {
        var value = Guid.NewGuid();
        var example = new Example(new ResultMessage($"{value}"));
        var json = JsonSerializer.Serialize(example, _options);

        Assert.That(json, Is.Not.Null);
        Assert.That(json, Is.EqualTo($"{{\"M\":\"{value}\"}}"));
    }

    [Test]
    public void ResultMessage_CanRoundTripNull_ToJson()
    {
        var example = new NullableExample(null);
        var json = JsonSerializer.Serialize(example, _options);
        var copy = JsonSerializer.Deserialize<NullableExample>(json, _options);

        Assert.That(json, Is.EqualTo("{\"M\":null}"));
        Assert.That(copy, Is.EqualTo(example));
    }
}
