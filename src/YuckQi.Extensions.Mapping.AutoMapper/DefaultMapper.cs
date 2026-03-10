using AutoMapper;
using AutoMapperIMapper = AutoMapper.IMapper;
using IMapper = YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces.IMapper;

namespace YuckQi.Extensions.Mapping.AutoMapper;

public class DefaultMapper : IMapper
{
    private readonly AutoMapperIMapper _mapper;

    public DefaultMapper(IConfigurationProvider configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _mapper = configuration.CreateMapper();
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
