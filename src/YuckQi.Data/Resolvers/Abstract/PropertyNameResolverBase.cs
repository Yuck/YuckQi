using System.Linq.Expressions;
using YuckQi.Data.Extensions;
using YuckQi.Data.Resolvers.Abstract.Interfaces;
using YuckQi.Data.Resolvers.Extensions;

namespace YuckQi.Data.Resolvers.Abstract;

public abstract class PropertyNameResolverBase : IPropertyNameResolver
{
    public abstract String Resolve(String path);

    public String Resolve<TDomainEntity, TPersistenceModel>(Expression<Func<TDomainEntity, Object>> expression)
    {
        var path = expression.GetPath();
        var result = Resolve(path);

        return result;
    }

    public IPropertyNameResolver WithMapping<TDomainEntity, TPersistenceModel>(Expression<Func<TDomainEntity, Object>> from, Expression<Func<TPersistenceModel, Object>> to)
    {
        return new MappedPropertyNameResolver(this, new Dictionary<String, String> { { from.GetPath(), to.GetPath() } });
    }
}
