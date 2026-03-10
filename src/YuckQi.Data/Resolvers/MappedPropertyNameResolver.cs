using System.Collections.Generic;
using YuckQi.Data.Resolvers.Abstract;
using YuckQi.Data.Resolvers.Abstract.Interfaces;

namespace YuckQi.Data.Resolvers;

public sealed class MappedPropertyNameResolver(IPropertyNameResolver inner, IReadOnlyDictionary<String, String> overrides) : PropertyNameResolverBase
{
    public override String Resolve(String path)
    {
        if (overrides.TryGetValue(path, out var value))
            return value;

        return inner.Resolve(path);
    }
}
