namespace YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

public interface IMapper
{
    Object Map(Object source, Object destination, Type sourceType, Type destinationType);

    Object Map(Object source, Type sourceType, Type destinationType);

    TDestination Map<TDestination>(Object source);

    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

    TDestination Map<TSource, TDestination>(TSource source);

    Object? MapOrNull(Object? source, Object destination, Type sourceType, Type destinationType);

    Object? MapOrNull(Object? source, Type sourceType, Type destinationType);

    TDestination? MapOrNull<TDestination>(Object? source) where TDestination : class;

    TDestination? MapOrNull<TSource, TDestination>(TSource? source, TDestination destination) where TSource : class where TDestination : class;

    TDestination? MapOrNull<TSource, TDestination>(TSource? source) where TSource : class where TDestination : class;
}
