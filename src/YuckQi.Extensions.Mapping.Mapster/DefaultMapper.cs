using Mapster;
using MapsterMapper;
using IMapper = YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces.IMapper;
using MapsterIMapper = MapsterMapper.IMapper;

namespace YuckQi.Extensions.Mapping.Mapster;

public class DefaultMapper : IMapper
{
    private readonly MapsterIMapper _mapper;

    public DefaultMapper(TypeAdapterConfig? configuration = null)
    {
        _mapper = configuration is not null ? new Mapper(configuration) : new Mapper();
    }

    public Object Map(Object source, Object destination, Type sourceType, Type destinationType)
    {
        return _mapper.Map(source, destination, sourceType, destinationType);
    }

    public Object Map(Object source, Type sourceType, Type destinationType)
    {
        return _mapper.Map(source, sourceType, destinationType);
    }

    public TDestination Map<TDestination>(Object source)
    {
        return _mapper.Map<TDestination>(source);
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        return _mapper.Map<TSource, TDestination>(source);
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        return _mapper.Map(source, destination);
    }
}
