using System.Text.Json;
using System.Text.Json.Serialization;
using YuckQi.Domain.Validation.JsonConverters.Internal;

namespace YuckQi.Domain.Validation.JsonConverters;

public class ResultJsonConverter : JsonConverter<Result>
{
    public override Result Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonPropertyHelper.EnsureTokenType(ref reader, JsonTokenType.StartObject, typeToConvert);

        var detail = default(IReadOnlyCollection<ResultDetail>);

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var name = reader.GetString();

            reader.Read();

            if (JsonPropertyHelper.IsName(options, name, nameof(Result.Detail)))
                detail = ResultDetailJsonConverter.ReadCollection(ref reader, options);
            else if (JsonPropertyHelper.IsName(options, name, nameof(Result.IsValid)))
                reader.Skip();
            else
                JsonPropertyHelper.SkipUnmapped(ref reader, options, name, typeToConvert);
        }

        return new Result(detail);
    }

    public override void Write(Utf8JsonWriter writer, Result value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        WriteDetailAndIsValid(writer, value, options);

        writer.WriteEndObject();
    }

    internal static void WriteDetailAndIsValid(Utf8JsonWriter writer, Result value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(JsonPropertyHelper.GetName(options, nameof(Result.Detail)));

        ResultDetailJsonConverter.WriteCollection(writer, value.Detail, options);

        JsonPropertyHelper.WriteValue(writer, options, nameof(Result.IsValid), value.IsValid);
    }
}

public class ResultJsonConverter<T> : JsonConverter<Result<T>>
{
    public override Result<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonPropertyHelper.EnsureTokenType(ref reader, JsonTokenType.StartObject, typeToConvert);

        var content = default(T);
        var detail = default(IReadOnlyCollection<ResultDetail>);

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var name = reader.GetString();

            reader.Read();

            if (JsonPropertyHelper.IsName(options, name, nameof(Result<T>.Content)))
                content = JsonPropertyHelper.ReadValue<T>(ref reader, options);
            else if (JsonPropertyHelper.IsName(options, name, nameof(Result<T>.Detail)))
                detail = ResultDetailJsonConverter.ReadCollection(ref reader, options);
            else if (JsonPropertyHelper.IsName(options, name, nameof(Result<T>.IsValid)))
                reader.Skip();
            else
                JsonPropertyHelper.SkipUnmapped(ref reader, options, name, typeToConvert);
        }

        return new Result<T>(content, detail);
    }

    public override void Write(Utf8JsonWriter writer, Result<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        JsonPropertyHelper.WriteValue(writer, options, nameof(Result<T>.Content), value.Content);

        ResultJsonConverter.WriteDetailAndIsValid(writer, value, options);

        writer.WriteEndObject();
    }
}
