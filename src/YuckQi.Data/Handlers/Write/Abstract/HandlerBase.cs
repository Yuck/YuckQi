using YuckQi.Data.Handlers.Internal;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.Handlers.Write.Abstract;

public abstract class HandlerBase<TDomainEntity>(IMapper? mapper = null) : HandlerBase<TDomainEntity, TDomainEntity>(mapper);

public abstract class HandlerBase<TDomainEntity, TData>(IMapper? mapper = null)
{
    protected IMapper? Mapper { get; } = mapper;

    protected virtual TData? MapToData(TDomainEntity? entity)
    {
        return DataMapper.Default.MapToTarget<TDomainEntity, TData>(entity, Mapper);
    }

    protected virtual IReadOnlyCollection<TData> MapToDataCollection(IEnumerable<TDomainEntity>? entities)
    {
        return DataMapper.Default.MapToTargetCollection<TDomainEntity, TData>(entities, Mapper);
    }
}
