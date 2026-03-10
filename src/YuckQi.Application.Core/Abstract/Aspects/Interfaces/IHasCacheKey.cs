using MediatR;

namespace YuckQi.Application.Core.Abstract.Aspects.Interfaces;

public interface IHasCacheKey : IBaseRequest
{
    String CacheKey { get; }
}
