using YuckQi.Application.Core.Behaviors.Caching;

namespace YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Abstract.Interfaces;

public interface ICacheDependencyGraph
{
    IReadOnlySet<CacheKey> GetExpandedCacheKeys(IEnumerable<CacheKey> keys);
}
