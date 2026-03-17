using YuckQi.Data.Resolvers.Abstract;
using YuckQi.Data.Resolvers.Abstract.Interfaces;

namespace YuckQi.Data.Resolvers;

public sealed class CompositePropertyNameResolver(IReadOnlyCollection<IPropertyNameResolver> resolvers) : PropertyNameResolverBase
{
    public override String Resolve(String path)
    {
        return resolvers.Aggregate(path, (current, resolver) => resolver.Resolve(current));
    }
}
