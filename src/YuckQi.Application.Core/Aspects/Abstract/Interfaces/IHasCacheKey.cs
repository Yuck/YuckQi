using MediatR;

namespace YuckQi.Application.Core.Aspects.Abstract.Interfaces;

public interface IHasCacheKey : IBaseRequest
{
    String CacheKey { get; }
}
