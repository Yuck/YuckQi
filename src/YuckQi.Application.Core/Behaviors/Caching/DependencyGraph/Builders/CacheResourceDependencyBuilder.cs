using YuckQi.Application.Core.Behaviors.Caching;

namespace YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Builders;

public sealed class CacheResourceDependencyBuilder(List<Func<CacheKeyContext, IEnumerable<CacheKey>>> factories)
{
    public CacheResourceDependencyBuilder Invalidates(String resource)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);

        factories.Add(CreateSameIdentifierFactory(resource));

        return this;
    }

    public CacheResourceDependencyBuilder Invalidates(Func<CacheKeyContext, CacheKey?> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        factories.Add(CreateSingleFactory(factory));

        return this;
    }

    public CacheResourceDependencyBuilder Invalidates(Func<CacheKeyContext, IEnumerable<CacheKey>> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        factories.Add(factory);

        return this;
    }

    public CacheResourceDependencyBuilder InvalidatesFromParameter(String resource, String parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        factories.Add(CreateParameterFactory(resource, parameterName));

        return this;
    }

    public CacheResourceDependencyBuilder InvalidatesGlobal(String resource)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);

        factories.Add(CreateGlobalFactory(resource));

        return this;
    }

    private static Func<CacheKeyContext, IEnumerable<CacheKey>> CreateGlobalFactory(String resource)
    {
        return CreateKeys;

        IEnumerable<CacheKey> CreateKeys(CacheKeyContext context)
        {
            yield return context.Global(resource);
        }
    }

    private static Func<CacheKeyContext, IEnumerable<CacheKey>> CreateParameterFactory(String resource, String parameterName)
    {
        return CreateKeys;

        IEnumerable<CacheKey> CreateKeys(CacheKeyContext context)
        {
            var identifier = context.Parameter(parameterName);
            if (identifier is null)
                yield break;

            yield return context.Key(resource, identifier);
        }
    }

    private static Func<CacheKeyContext, IEnumerable<CacheKey>> CreateSameIdentifierFactory(String resource)
    {
        return CreateKeys;

        IEnumerable<CacheKey> CreateKeys(CacheKeyContext context)
        {
            yield return context.Key(resource);
        }
    }

    private static Func<CacheKeyContext, IEnumerable<CacheKey>> CreateSingleFactory(Func<CacheKeyContext, CacheKey?> factory)
    {
        return CreateKeys;

        IEnumerable<CacheKey> CreateKeys(CacheKeyContext context)
        {
            var key = factory(context);
            if (key is not { } cacheKey)
                yield break;

            yield return cacheKey;
        }
    }
}
