using YuckQi.Application.Core.Behaviors.Caching;
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Factories;

namespace YuckQi.Application.Core.Behaviors.Caching.DependencyGraph;

public sealed class CacheKeyContext(CacheKeyParts parts)
{
    public String? Identifier { get; } = parts.Identifier;

    public IReadOnlyDictionary<String, String> Parameters { get; } = parts.Parameters;

    public String Resource { get; } = parts.Resource;

    public CacheKey Global(String resource)
    {
        return CacheKeyFactory.Create(resource);
    }

    public CacheKey Key(String resource)
    {
        if (Identifier is null)
            return CacheKeyFactory.Create(resource);

        return CacheKeyFactory.Create(resource, Identifier);
    }

    public CacheKey Key(String resource, Object identifier)
    {
        return CacheKeyFactory.Create(resource, identifier);
    }

    public String? Parameter(String name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return Parameters.TryGetValue(name, out var value) ? value : null;
    }
}
