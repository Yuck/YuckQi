using System.Text.Json;
using NUnit.Framework;
using YuckQi.Domain.Validation.Extensions;
using YuckQi.Domain.Validation.JsonConverters;
using YuckQi.Domain.Validation.JsonConverters.Factories;

namespace YuckQi.Domain.Validation.UnitTests.Extensions;

public class JsonSerializerOptionsExtensionsTests
{
    [Test]
    public void JsonSerializerOptions_AddResultJsonConverters_ReturnsSameInstance()
    {
        var options = new JsonSerializerOptions();
        var result = options.AddResultJsonConverters();

        Assert.That(result, Is.SameAs(options));
    }

    [Test]
    public void JsonSerializerOptions_AddResultJsonConverters_AddsConverters()
    {
        var options = new JsonSerializerOptions().AddResultJsonConverters();

        Assert.That(options.Converters, Has.Some.TypeOf<ResultCodeJsonConverter>());
        Assert.That(options.Converters, Has.Some.TypeOf<ResultDetailJsonConverter>());
        Assert.That(options.Converters, Has.Some.TypeOf<ResultJsonConverter>());
        Assert.That(options.Converters, Has.Some.TypeOf<ResultJsonConverterFactory>());
        Assert.That(options.Converters, Has.Some.TypeOf<ResultMessageJsonConverter>());
    }

    [Test]
    public void JsonSerializerOptions_AddResultJsonConverters_KeepsExistingConverters()
    {
        var existing = new ResultCodeJsonConverter();
        var options = new JsonSerializerOptions { Converters = { existing } }.AddResultJsonConverters();

        Assert.That(options.Converters, Does.Contain(existing));
    }
}
