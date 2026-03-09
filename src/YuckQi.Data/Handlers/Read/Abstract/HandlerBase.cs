using YuckQi.Data.Handlers.Internal;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.Handlers.Read.Abstract;

public abstract class HandlerBase<TDomainEntity>(IMapper? mapper = null) : HandlerBase<TDomainEntity, TDomainEntity>(mapper);

public abstract class HandlerBase<TDomainEntity, TData>(IMapper? mapper = null)
{
    protected IMapper? Mapper { get; } = mapper;

    protected virtual TDomainEntity? MapToEntity(TData? data)
    {
        return DataMapper.Default.MapToTarget<TData, TDomainEntity>(data, Mapper);
    }

    protected virtual IReadOnlyCollection<TDomainEntity> MapToEntityCollection(IEnumerable<TData>? data)
    {
        return DataMapper.Default.MapToTargetCollection<TData, TDomainEntity>(data, Mapper);
    }
}
