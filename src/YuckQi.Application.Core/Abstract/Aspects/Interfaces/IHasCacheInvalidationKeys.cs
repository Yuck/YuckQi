namespace YuckQi.Application.Core.Abstract.Aspects.Interfaces;

public interface IHasCacheInvalidationKeys
{
    IReadOnlySet<String> CacheKeys { get; }
}
