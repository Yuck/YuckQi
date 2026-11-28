using System.Text.Json;

namespace YuckQi.Application.Core.Behaviors.Caching;

public record DistributedCachingBehaviorOptions
{
    public TimeSpan? CacheDuration { get; set; }
    public JsonSerializerOptions? SerializerOptions { get; set; }

    public DistributedCachingBehaviorOptions() { }

    public DistributedCachingBehaviorOptions(TimeSpan? cacheDuration)
    {
        CacheDuration = cacheDuration;
    }
}
