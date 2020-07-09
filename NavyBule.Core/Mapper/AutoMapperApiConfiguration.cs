using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration;

namespace NavyBule.Core.Mapper
{
    /// <summary>
    /// Class AutoMapperApiConfiguration.
    /// </summary>
    public static class AutoMapperApiConfiguration
    {
        /// <summary>
        /// The s mapper configuration expression
        /// </summary>
        private static MapperConfigurationExpression _mapperConfigurationExpression;
        private static IMapper _mapper;
        private static readonly object _mapperLockObject = new object();
        public static MapperConfiguration _mapperConfiguration { get; private set; }
        public static void Init(MapperConfiguration config)
        {
               _mapperConfiguration = config;
            _mapper = config.CreateMapper();
        }
        /// <summary>
        /// Gets the mapper configuration expression.
        /// </summary>
        /// <value>The mapper configuration expression.</value>
        public static MapperConfigurationExpression MapperConfigurationExpression
        {
            get
            {
                var expression = _mapperConfigurationExpression;
                if (expression != null)
                {
                    return expression;
                }

                return (_mapperConfigurationExpression = new MapperConfigurationExpression());
            }
        }

        /// <summary>
        /// Gets the mapper.
        /// </summary>
        /// <value>The mapper.</value>
        public static IMapper Mapper
        {
            get
            {
                if (_mapper == null)
                {
                    lock (_mapperLockObject)
                    {
                        if (_mapper == null)
                        {
                            var mapperConfiguration = new MapperConfiguration(MapperConfigurationExpression);

                            _mapper = mapperConfiguration.CreateMapper();
                        }
                    }
                }

                return _mapper;
            }
        }

        /// <summary>
        /// Maps to.
        /// </summary>
        /// <typeparam name="TSource">The type of the t source.</typeparam>
        /// <typeparam name="TDestination">The type of the t destination.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>TDestination.</returns>
        public static TDestination MapTo<TSource, TDestination>(this TSource source)
        {
            return Mapper.Map<TSource, TDestination>(source);
        }

        /// <summary>
        /// Maps to.
        /// </summary>
        /// <typeparam name="TSource">The type of the t source.</typeparam>
        /// <typeparam name="TDestination">The type of the t destination.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <returns>TDestination.</returns>
        public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            return Mapper.Map(source, destination);
        }
    }
}
