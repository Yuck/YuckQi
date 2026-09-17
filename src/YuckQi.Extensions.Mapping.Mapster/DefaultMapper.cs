using Mapster;
using MapsterMapper;
using IMapper = YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces.IMapper;
using MapsterIMapper = MapsterMapper.IMapper;

namespace YuckQi.Extensions.Mapping.Mapster;

public class DefaultMapper(TypeAdapterConfig? configuration = null) : IMapper
{
    private readonly MapsterIMapper _mapper = configuration is not null ? new Mapper(configuration) : new Mapper();

    public Object Map(Object source, Object destination, Type sourceType, Type destinationType)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        var result = _mapper.Map(source, destination, sourceType, destinationType);
        if (result is null)
            throw new InvalidOperationException($"Mapping from '{sourceType}' to '{destinationType}' produced a null result.");

        return result;
    }

    public Object Map(Object source, Type sourceType, Type destinationType)
    {
        ArgumentNullException.ThrowIfNull(source);

        var result = _mapper.Map(source, sourceType, destinationType);
        if (result is null)
            throw new InvalidOperationException($"Mapping from '{sourceType}' to '{destinationType}' produced a null result.");

        return result;
    }

    public TDestination Map<TDestination>(Object source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var result = _mapper.Map<TDestination>(source);
        if (result is null)
            throw new InvalidOperationException($"Mapping to '{typeof(TDestination)}' produced a null result.");

        return result;
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        var result = _mapper.Map(source, destination);
        if (result is null)
            throw new InvalidOperationException($"Mapping from '{typeof(TSource)}' to '{typeof(TDestination)}' produced a null result.");

        return result;
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var result = _mapper.Map<TSource, TDestination>(source);
        if (result is null)
            throw new InvalidOperationException($"Mapping from '{typeof(TSource)}' to '{typeof(TDestination)}' produced a null result.");

        return result;
    }

    public Object? MapOrNull(Object? source, Object destination, Type sourceType, Type destinationType)
    {
        ArgumentNullException.ThrowIfNull(destination);

        if (source is null)
            return null;

        return Map(source, destination, sourceType, destinationType);
    }

    public Object? MapOrNull(Object? source, Type sourceType, Type destinationType)
    {
        if (source is null)
            return null;

        return Map(source, sourceType, destinationType);
    }

    public TDestination? MapOrNull<TDestination>(Object? source) where TDestination : class
    {
        if (source is null)
            return null;

        return Map<TDestination>(source);
    }

    public TDestination? MapOrNull<TSource, TDestination>(TSource? source, TDestination destination) where TSource : class where TDestination : class
    {
        ArgumentNullException.ThrowIfNull(destination);

        if (source is null)
            return null;

        return Map(source, destination);
    }

    public TDestination? MapOrNull<TSource, TDestination>(TSource? source) where TSource : class where TDestination : class
    {
        if (source is null)
            return null;

        return Map<TSource, TDestination>(source);
    }
}
