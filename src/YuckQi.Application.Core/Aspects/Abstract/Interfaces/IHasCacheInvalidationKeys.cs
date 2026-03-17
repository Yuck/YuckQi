namespace YuckQi.Application.Core.Aspects.Abstract.Interfaces;

public interface IHasCacheInvalidationKeys
{
    IReadOnlySet<String> CacheKeys { get; }
}
