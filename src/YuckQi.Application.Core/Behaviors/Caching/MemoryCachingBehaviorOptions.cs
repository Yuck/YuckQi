using System.Text.Json;

namespace YuckQi.Application.Core.Behaviors.Caching;

public record MemoryCachingBehaviorOptions
{
    private Int64 _size = 1;

    public TimeSpan? CacheDuration { get; set; }
    public JsonSerializerOptions? SerializerOptions { get; set; }

    public Int64 Size
    {
        get => _size;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);

            _size = value;
        }
    }

    public MemoryCachingBehaviorOptions() { }

    public MemoryCachingBehaviorOptions(TimeSpan? cacheDuration)
    {
        CacheDuration = cacheDuration;
    }
}
