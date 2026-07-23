using Mediator;

namespace YuckQi.Application.Core.Aspects.Abstract.Interfaces;

public interface IHasCacheKey : IMessage
{
    String CacheKey { get; }
}
