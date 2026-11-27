using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace YuckQi.Domain.Validation.JsonConverters.Internal;

internal static class JsonPropertyHelper
{
    public static void EnsureTokenType(ref Utf8JsonReader reader, JsonTokenType tokenType, Type typeToConvert)
    {
        if (reader.TokenType == tokenType)
            return;

        throw new JsonException($"Expected '{tokenType}' but found '{reader.TokenType}' while reading '{typeToConvert.Name}'.");
    }

    public static String GetName(JsonSerializerOptions options, String name)
    {
        if (options.PropertyNamingPolicy is null)
            return name;

        return options.PropertyNamingPolicy.ConvertName(name);
    }

    public static Boolean IsName(JsonSerializerOptions options, String? actual, String name)
    {
        var comparison = options.PropertyNameCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        return String.Equals(actual, GetName(options, name), comparison);
    }

    public static TValue? ReadValue<TValue>(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize(ref reader, GetTypeInfo<TValue>(options));
    }

    public static void SkipUnmapped(ref Utf8JsonReader reader, JsonSerializerOptions options, String? name, Type typeToConvert)
    {
        if (options.UnmappedMemberHandling == JsonUnmappedMemberHandling.Disallow)
            throw new JsonException($"The JSON property '{name}' could not be mapped to any member of '{typeToConvert.Name}'.");

        reader.Skip();
    }

    public static void WriteValue<TValue>(Utf8JsonWriter writer, JsonSerializerOptions options, String name, TValue value)
    {
        if (! ShouldWrite(options, value is null, EqualityComparer<TValue>.Default.Equals(value, default)))
            return;

        writer.WritePropertyName(GetName(options, name));

        JsonSerializer.Serialize(writer, value, GetTypeInfo<TValue>(options));
    }

    private static JsonTypeInfo<TValue> GetTypeInfo<TValue>(JsonSerializerOptions options)
    {
        return (JsonTypeInfo<TValue>) options.GetTypeInfo(typeof(TValue));
    }

    private static Boolean ShouldWrite(JsonSerializerOptions options, Boolean isNull, Boolean isDefault)
    {
        return options.DefaultIgnoreCondition switch
        {
            JsonIgnoreCondition.WhenWritingNull => ! isNull,
            JsonIgnoreCondition.WhenWritingDefault => ! isDefault,
            _ => true
        };
    }
}
