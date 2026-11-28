using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using YuckQi.Application.Core.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Validation.Extensions;

namespace YuckQi.Application.Core.Behaviors.Caching.Internal;

internal static class CachedResponse
{
    private const String TypePropertyName = "type";
    private const String ValuePropertyName = "value";

    private static readonly JsonSerializerOptions DefaultSerializerOptions = new JsonSerializerOptions().AddResultJsonConverters();

    public static Boolean HasValidationErrors<TResponse>(TResponse response)
    {
        if (response is not IHasValidationResults { ValidationResults: { } results })
            return false;

        return results.Any(t => t is not null && ! t.IsValid);
    }

    public static Boolean TryDeserialize<TResponse>(Byte[] bytes, JsonSerializerOptions? options, [MaybeNullWhen(false)] out TResponse response)
    {
        response = default;

        using var document = JsonDocument.Parse(bytes);

        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            return false;
        if (! root.TryGetProperty(TypePropertyName, out var type))
            return false;
        if (type.ValueKind != JsonValueKind.String)
            return false;
        if (! String.Equals(type.GetString(), GetTypeName<TResponse>(), StringComparison.Ordinal))
            return false;
        if (! root.TryGetProperty(ValuePropertyName, out var value))
            return false;

        var result = value.Deserialize<TResponse>(GetSerializerOptions(options));
        if (result is null)
            return false;

        response = result;

        return true;
    }

    public static Boolean TrySerialize<TResponse>(TResponse response, JsonSerializerOptions? options, [NotNullWhen(true)] out Byte[]? bytes)
    {
        bytes = null;

        if (response is null)
            return false;

        var resolved = GetSerializerOptions(options);
        var value = JsonSerializer.SerializeToUtf8Bytes(response, resolved);
        if (! IsSameJson(value, JsonSerializer.SerializeToUtf8Bytes(response, response.GetType(), resolved)))
            return false;

        var copy = JsonSerializer.Deserialize<TResponse>(value, resolved);
        if (copy is null)
            return false;
        if (! IsSameJson(value, JsonSerializer.SerializeToUtf8Bytes(copy, resolved)))
            return false;

        bytes = Wrap<TResponse>(value);

        return true;
    }

    private static JsonSerializerOptions GetSerializerOptions(JsonSerializerOptions? options)
    {
        if (options is null)
            return DefaultSerializerOptions;

        return options;
    }

    private static String GetTypeName<TResponse>()
    {
        return typeof(TResponse).ToString();
    }

    private static Boolean IsSameJson(Byte[] expected, Byte[] actual)
    {
        return expected.AsSpan().SequenceEqual(actual);
    }

    private static Byte[] Wrap<TResponse>(Byte[] value)
    {
        var buffer = new ArrayBufferWriter<Byte>();

        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString(TypePropertyName, GetTypeName<TResponse>());
            writer.WritePropertyName(ValuePropertyName);
            writer.WriteRawValue(value, true);
            writer.WriteEndObject();
        }

        return buffer.WrittenSpan.ToArray();
    }
}
