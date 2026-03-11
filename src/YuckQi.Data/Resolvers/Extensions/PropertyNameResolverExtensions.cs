using YuckQi.Data.Resolvers.Abstract.Interfaces;

namespace YuckQi.Data.Resolvers.Extensions;

public static class PropertyNameResolverExtensions
{
    public static IPropertyNameResolver ToResolver(this IEnumerable<IPropertyNameResolver> resolvers)
    {
        var list = resolvers.ToList();

        return list.Count switch
        {
            0 => new DefaultPropertyNameResolver(),
            1 => list.First(),
            _ => new CompositePropertyNameResolver(list)
        };
    }
}
