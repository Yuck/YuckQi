using System.Text.Json;
using YuckQi.Domain.Validation.JsonConverters;
using YuckQi.Domain.Validation.JsonConverters.Factories;

namespace YuckQi.Domain.Validation.Extensions;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions AddResultJsonConverters(this JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Converters.Add(new ResultJsonConverterFactory());

        options.Converters.Add(new ResultCodeJsonConverter());
        options.Converters.Add(new ResultDetailJsonConverter());
        options.Converters.Add(new ResultJsonConverter());
        options.Converters.Add(new ResultMessageJsonConverter());

        return options;
    }
}
