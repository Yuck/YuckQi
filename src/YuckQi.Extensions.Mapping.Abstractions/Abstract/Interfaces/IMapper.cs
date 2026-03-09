namespace YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

public interface IMapper
{
    Object Map(Object source, Object destination, Type sourceType, Type destinationType);
    Object Map(Object source, Type sourceType, Type destinationType);
    TDestination Map<TDestination>(Object source);
    TDestination Map<TSource, TDestination>(TSource source);
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
}
