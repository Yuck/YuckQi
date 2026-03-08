using System;
using AutoMapper;
using AutoMapperIMapper = AutoMapper.IMapper;
using IMapper = YuckQi.Extensions.Mapping.Abstractions.IMapper;

namespace YuckQi.Extensions.Mapping.AutoMapper
{
    public class DefaultMapper : IMapper
    {
        #region Private Members

        private readonly AutoMapperIMapper _mapper;

        #endregion


        #region Constructors

        public DefaultMapper(IConfigurationProvider configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _mapper = configuration.CreateMapper();
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
