using System;
using Mapster;
using MapsterMapper;
using IMapper = YuckQi.Extensions.Mapping.Abstractions.IMapper;
using MapsterIMapper = MapsterMapper.IMapper;

namespace YuckQi.Extensions.Mapping.Mapster
{
    public class DefaultMapper : IMapper
    {
        #region Private Members

        private readonly MapsterIMapper _mapper;

        #endregion


        #region Constructors

        public DefaultMapper(TypeAdapterConfig configuration = null)
        {
            _mapper = configuration != null ? new Mapper(configuration) : new Mapper();
        }

        #endregion


        #region Public Methods

        public Object Map(Object source, Object destination, Type sourceType, Type destinationType) => _mapper.Map(source, destination, sourceType, destinationType);

        public Object Map(Object source, Type sourceType, Type destinationType) => _mapper.Map(source, sourceType, destinationType);

        public TDestination Map<TDestination>(Object source) => _mapper.Map<TDestination>(source);

        public TDestination Map<TSource, TDestination>(TSource source) => _mapper.Map<TSource, TDestination>(source);

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) => _mapper.Map(source, destination);

        #endregion
    }
}
