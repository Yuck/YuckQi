using System.Text.Json;
using System.Text.Json.Serialization;
using YuckQi.Domain.Validation.JsonConverters.Internal;

namespace YuckQi.Domain.Validation.JsonConverters;

public class ResultDetailJsonConverter : JsonConverter<ResultDetail>
{
    public override ResultDetail Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadDetail(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, ResultDetail value, JsonSerializerOptions options)
    {
        WriteDetail(writer, value, options);
    }

    internal static IReadOnlyCollection<ResultDetail> ReadCollection(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return [];

        JsonPropertyHelper.EnsureTokenType(ref reader, JsonTokenType.StartArray, typeof(IReadOnlyCollection<ResultDetail>));

        var detail = new List<ResultDetail>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            detail.Add(ReadDetail(ref reader, options));

        return detail;
    }

    internal static void WriteCollection(Utf8JsonWriter writer, IReadOnlyCollection<ResultDetail> detail, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var item in detail)
            WriteDetail(writer, item, options);

        writer.WriteEndArray();
    }

    private static ResultDetail ReadDetail(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        JsonPropertyHelper.EnsureTokenType(ref reader, JsonTokenType.StartObject, typeof(ResultDetail));

        var code = default(String?);
        var message = default(String?);
        var property = default(String?);
        var type = ResultType.Unknown;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var name = reader.GetString();

            reader.Read();

            if (JsonPropertyHelper.IsName(options, name, nameof(ResultDetail.Code)))
                code = JsonPropertyHelper.ReadValue<String>(ref reader, options);
            else if (JsonPropertyHelper.IsName(options, name, nameof(ResultDetail.Message)))
                message = JsonPropertyHelper.ReadValue<String>(ref reader, options);
            else if (JsonPropertyHelper.IsName(options, name, nameof(ResultDetail.Property)))
                property = JsonPropertyHelper.ReadValue<String>(ref reader, options);
            else if (JsonPropertyHelper.IsName(options, name, nameof(ResultDetail.Type)))
                type = JsonPropertyHelper.ReadValue<ResultType>(ref reader, options);
            else
                JsonPropertyHelper.SkipUnmapped(ref reader, options, name, typeof(ResultDetail));
        }

        return new ResultDetail(message, code, property, type);
    }

    private static void WriteDetail(Utf8JsonWriter writer, ResultDetail value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        JsonPropertyHelper.WriteValue<String?>(writer, options, nameof(ResultDetail.Code), value.Code);
        JsonPropertyHelper.WriteValue<String?>(writer, options, nameof(ResultDetail.Message), value.Message);
        JsonPropertyHelper.WriteValue(writer, options, nameof(ResultDetail.Property), value.Property);
        JsonPropertyHelper.WriteValue(writer, options, nameof(ResultDetail.Type), value.Type);

        writer.WriteEndObject();
    }
}
