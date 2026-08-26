using YuckQi.Application.Core.Behaviors.Caching;
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Abstract.Interfaces;

namespace YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Builders;

public sealed class CacheDependencyGraphBuilder
{
    private readonly Dictionary<String, List<Func<CacheKeyContext, IEnumerable<CacheKey>>>> _dependencies = new(StringComparer.Ordinal);

    public ICacheDependencyGraph Build()
    {
        var dependencies = _dependencies.ToDictionary(t => t.Key, t => (IReadOnlyList<Func<CacheKeyContext, IEnumerable<CacheKey>>>) t.Value.ToArray(), StringComparer.Ordinal);

        return new CacheDependencyGraph(dependencies);
    }

    public CacheDependencyGraphBuilder When(String resource, Action<CacheResourceDependencyBuilder> configure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);
        ArgumentNullException.ThrowIfNull(configure);

        if ( ! _dependencies.TryGetValue(resource, out var factories))
        {
            factories = [];

            _dependencies[resource] = factories;
        }

        configure(new CacheResourceDependencyBuilder(factories));

        return this;
    }
}
