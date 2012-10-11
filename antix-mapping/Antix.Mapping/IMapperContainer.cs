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
        IMapperContainer RegisterMapper<TFrom, TTo>(
            Action<TFrom, TTo, IMapperContainer> action);

        /// <summary>
        ///   <para> Register the creator </para>
        /// </summary>
        /// <typeparam name="T"> Type to create </typeparam>
        /// <param name="func"> Creator function </param>
        /// <returns> This Container </returns>
        IMapperContainer RegisterCreator<T>(Func<Type, T> func);

        /// <summary>
        ///   <para> Register the updater </para>
        /// </summary>
        /// <typeparam name="T"> Type to update </typeparam>
        /// <param name="action"> Updater function </param>
        /// <returns> This Container </returns>
        IMapperContainer RegisterUpdater<T>(Action<T> action);

        /// <summary>
        ///   <para> Register the deteter </para>
        /// </summary>
        /// <typeparam name="T"> Type to detete </typeparam>
        /// <param name="action"> Deleter function </param>
        /// <returns> This Container </returns>
        IMapperContainer RegisterDeleter<T>(Action<T> action);

        /// <summary>
        ///   <para> Check the mapper exists in this container </para>
        /// </summary>
        /// <typeparam name="TFrom"> Type to map from </typeparam>
        /// <typeparam name="TTo"> Type to map to </typeparam>
        /// <returns> True if found </returns>
        bool ContainsMapper<TFrom, TTo>();

        /// <summary>
        ///   <para> Get a registered mapper </para>
        /// </summary>
        /// <typeparam name="TFrom"> Type to map from </typeparam>
        /// <typeparam name="TTo"> Type to map to </typeparam>
        /// <returns> Mapper </returns>
        Action<TFrom, TTo, IMapperContainer> GetMapper<TFrom, TTo>();

        /// <summary>
        ///   <para> Map an object </para>
        /// </summary>
        /// <typeparam name="TFrom"> Type to map from </typeparam>
        /// <typeparam name="TTo"> Type to map to </typeparam>
        /// <param name="from"> Object to map from </param>
        /// <param name="toExpression"> Expression for the target </param>
        /// <param name="createToItem"> Create a new to item </param>
        void Map<TFrom, TTo>(
            TFrom from,
            Expression<Func<TTo>> toExpression,
            Func<TTo> createToItem)
            where TTo : class;

        /// <summary>
        ///   <para> Map all items </para>
        /// </summary>
        /// <typeparam name="TFrom"> Type to map from </typeparam>
        /// <typeparam name="TTo"> Type to map to </typeparam>
        /// <param name="from"> Object enumerable to map from </param>
        /// <param name="toExpression"> Object collection to map to </param>
        /// <param name="match"> Match function to find objects in the 'to' items given a 'from' item </param>
        void MapAll<TFrom, TTo>(
            IEnumerable<TFrom> from,
            Expression<Func<IEnumerable<TTo>>> toExpression,
            Func<TFrom, TTo, bool> match)
            where TTo : class;
    }
}