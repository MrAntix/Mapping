using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Antix.Mapping
{
    public interface IMapperContainer
    {
        /// <summary>
        ///   <para> Register a mapping </para>
        /// </summary>
        /// <typeparam name="TFrom"> Type to map from </typeparam>
        /// <typeparam name="TTo"> Type to map to </typeparam>
        /// <param name="action"> Mapping </param>
        /// <returns> This container </returns>
        IMapperContainer Register<TFrom, TTo>(
            Action<TFrom, TTo, IMapperContext> action);

        /// <summary>
        ///   <para> Check the mapper exists in this container </para>
        /// </summary>
        /// <typeparam name="TFrom"> Type to map from </typeparam>
        /// <typeparam name="TTo"> Type to map to </typeparam>
        /// <returns> True if found </returns>
        bool Contains<TFrom, TTo>();

        /// <summary>
        ///   <para> Get a registered mapper </para>
        /// </summary>
        /// <typeparam name="TFrom"> Type to map from </typeparam>
        /// <typeparam name="TTo"> Type to map to </typeparam>
        /// <returns> Mapper </returns>
        Action<TFrom, TTo, IMapperContext> Get<TFrom, TTo>();

        /// <summary>
        ///   <para> Map an object </para>
        /// </summary>
        /// <typeparam name="TFrom"> Type to map from </typeparam>
        /// <typeparam name="TTo"> Type to map to </typeparam>
        /// <param name="from"> Object to map from </param>
        /// <param name="toExpression"> Expression for the target </param>
        /// <param name="context"> Context </param>
        void Map<TFrom, TTo>(
            TFrom from,
            Expression<Func<TTo>> toExpression,
            IMapperContext context)
            where TTo : class;

        /// <summary>
        ///   <para> Map all items </para>
        /// </summary>
        /// <typeparam name="TFrom"> Type to map from </typeparam>
        /// <typeparam name="TTo"> Type to map to </typeparam>
        /// <param name="from"> Object enumerable to map from </param>
        /// <param name="toExpression"> Object collection to map to </param>
        /// <param name="match"> Match function to find objects in the 'to' items given a 'from' item </param>
        /// <param name="context"> Context </param>
        void MapAll<TFrom, TTo>(
            IEnumerable<TFrom> from,
            Expression<Func<IEnumerable<TTo>>> toExpression,
            Func<TFrom, TTo, bool> match,
            IMapperContext context)
            where TTo : class;
    }
}