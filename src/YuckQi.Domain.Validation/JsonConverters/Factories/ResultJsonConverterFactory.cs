using System.Text.Json;
using System.Text.Json.Serialization;

namespace YuckQi.Domain.Validation.JsonConverters.Factories;

public class ResultJsonConverterFactory : JsonConverterFactory
{
    public override Boolean CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Result<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(ResultJsonConverter<>).MakeGenericType(typeToConvert.GetGenericArguments());

        return Activator.CreateInstance(converterType) as JsonConverter;
    }
}
