using System.Linq.Expressions;

namespace YuckQi.Data.Resolvers.Abstract.Interfaces;

public interface IPropertyNameResolver
{
    String Resolve(String path);

    String Resolve<TDomainEntity, TPersistenceModel>(Expression<Func<TDomainEntity, Object>> expression);
}
