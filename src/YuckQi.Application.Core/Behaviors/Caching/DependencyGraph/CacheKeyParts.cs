using YuckQi.Application.Core.Behaviors.Caching;
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Factories;

namespace YuckQi.Application.Core.Behaviors.Caching.DependencyGraph;

public sealed record CacheKeyParts(String Resource, String? Identifier, IReadOnlyDictionary<String, String> Parameters)
{
    public CacheKey ToKey()
    {
        if (Identifier is null)
            return CacheKeyFactory.Create(Resource);

        if (Parameters.Count == 0)
            return CacheKeyFactory.Create(Resource, Identifier);

        var parameters = Parameters.Select(t => (t.Key, (Object) t.Value)).ToArray();

        return CacheKeyFactory.Create(Resource, Identifier, parameters);
    }
}
