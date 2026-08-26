using YuckQi.Application.Core.Behaviors.Caching;
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Abstract.Interfaces;
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Builders;
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Factories;

namespace YuckQi.Application.Core.Behaviors.Caching.DependencyGraph;

public sealed class CacheDependencyGraph : ICacheDependencyGraph
{
    private readonly IReadOnlyDictionary<String, IReadOnlyList<Func<CacheKeyContext, IEnumerable<CacheKey>>>> _dependencies;

    internal CacheDependencyGraph(IReadOnlyDictionary<String, IReadOnlyList<Func<CacheKeyContext, IEnumerable<CacheKey>>>> dependencies)
    {
        _dependencies = dependencies;
    }

    public static ICacheDependencyGraph Empty { get; } = new CacheDependencyGraph(new Dictionary<String, IReadOnlyList<Func<CacheKeyContext, IEnumerable<CacheKey>>>>());

    public static ICacheDependencyGraph Create(Action<CacheDependencyGraphBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new CacheDependencyGraphBuilder();

        configure(builder);

        return builder.Build();
    }

    public IReadOnlySet<CacheKey> GetExpandedCacheKeys(IEnumerable<CacheKey> keys)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var expanded = new HashSet<CacheKey>();
        var queue = new Queue<CacheKey>();

        foreach (var key in keys)
        {
            if (String.IsNullOrWhiteSpace(key))
                continue;

            if (expanded.Add(key))
                queue.Enqueue(key);
        }

        while (queue.Count > 0)
        {
            var key = queue.Dequeue();
            if ( ! CacheKeyFactory.TryParse(key, out var parts))
                continue;

            if ( ! _dependencies.TryGetValue(parts.Resource, out var factories))
                continue;

            var context = new CacheKeyContext(parts);
            foreach (var factory in factories)
            {
                foreach (var dependent in factory(context))
                {
                    if (String.IsNullOrWhiteSpace(dependent))
                        continue;

                    if (expanded.Add(dependent))
                        queue.Enqueue(dependent);
                }
            }
        }

        return expanded;
    }
}
