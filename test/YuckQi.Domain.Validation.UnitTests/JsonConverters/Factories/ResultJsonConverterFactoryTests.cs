using System.Text.Json;
using NUnit.Framework;
using YuckQi.Domain.Validation.JsonConverters;
using YuckQi.Domain.Validation.JsonConverters.Factories;

namespace YuckQi.Domain.Validation.UnitTests.JsonConverters.Factories;

public class ResultJsonConverterFactoryTests
{
    [Test]
    public void ResultJsonConverterFactory_CanConvert_GenericResultOnly()
    {
        var factory = new ResultJsonConverterFactory();

        Assert.That(factory.CanConvert(typeof(Result<String>)), Is.True);
        Assert.That(factory.CanConvert(typeof(Result)), Is.False);
        Assert.That(factory.CanConvert(typeof(List<String>)), Is.False);
        Assert.That(factory.CanConvert(typeof(String)), Is.False);
    }

    [Test]
    public void ResultJsonConverterFactory_CreateConverter_ReturnsTypedConverter()
    {
        var factory = new ResultJsonConverterFactory();
        var converter = factory.CreateConverter(typeof(Result<Guid>), new JsonSerializerOptions());

        Assert.That(converter, Is.TypeOf<ResultJsonConverter<Guid>>());
    }
}
