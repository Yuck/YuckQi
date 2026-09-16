using Mapster;
using MapsterMapper;
using IMapper = YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces.IMapper;
using MapsterIMapper = MapsterMapper.IMapper;

namespace YuckQi.Extensions.Mapping.Mapster;

public class DefaultMapper(TypeAdapterConfig? configuration = null) : IMapper
{
    private readonly MapsterIMapper _mapper = configuration is not null ? new Mapper(configuration) : new Mapper();

    public Object? Map(Object? source, Object destination, Type sourceType, Type destinationType)
    {
        ArgumentNullException.ThrowIfNull(destination);

        if (source is null)
            return null;

        return _mapper.Map(source, destination, sourceType, destinationType);
    }

    public Object? Map(Object? source, Type sourceType, Type destinationType)
    {
        if (source is null)
            return null;

        return _mapper.Map(source, sourceType, destinationType);
    }

    public TDestination? Map<TDestination>(Object? source)
    {
        if (source is null)
            return default;

        return _mapper.Map<TDestination>(source);
    }

    public TDestination? Map<TSource, TDestination>(TSource? source)
    {
        if (source is null)
            return default;

        return _mapper.Map<TSource, TDestination>(source);
    }

    public TDestination? Map<TSource, TDestination>(TSource? source, TDestination destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        if (source is null)
            return default;

        return _mapper.Map(source, destination);
    }
}
