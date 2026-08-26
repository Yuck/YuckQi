using Mediator;
using YuckQi.Application.Core.Behaviors.Caching;

namespace YuckQi.Application.Core.Aspects.Abstract.Interfaces;

public interface IHasCacheKey : IMessage
{
    CacheKey CacheKey { get; }
}
