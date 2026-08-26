using YuckQi.Application.Core.Behaviors.Caching;

namespace YuckQi.Application.Core.Aspects.Abstract.Interfaces;

public interface IHasCacheInvalidationKeys
{
    IReadOnlySet<CacheKey> CacheKeys { get; }
}
