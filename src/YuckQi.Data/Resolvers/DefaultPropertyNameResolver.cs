using YuckQi.Data.Resolvers.Abstract;

namespace YuckQi.Data.Resolvers;

public sealed class DefaultPropertyNameResolver : PropertyNameResolverBase
{
    public override String Resolve(String path)
    {
        return path;
    }
}
