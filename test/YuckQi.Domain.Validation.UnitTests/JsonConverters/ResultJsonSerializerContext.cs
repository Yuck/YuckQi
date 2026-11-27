using System.Text.Json.Serialization;

namespace YuckQi.Domain.Validation.UnitTests.JsonConverters;

[JsonSerializable(typeof(Result))]
[JsonSerializable(typeof(Result<String>))]
public partial class ResultJsonSerializerContext : JsonSerializerContext;
