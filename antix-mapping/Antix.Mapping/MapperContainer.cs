using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Antix.Mapping.Expressions;

namespace Antix.Mapping
{
    public class MapperContainer : IMapperContainer
    {
        static readonly object LockObject = new object();

        readonly IDictionary<Tuple<Type, Type>, object>
            _mappers = new Dictionary<Tuple<Type, Type>, object>();

        /// <summary>
        ///   <para> Register a mapping </para>
        /// </summary>
        /// <typeparam name="TFrom"> Type to map from </typeparam>
        /// <typeparam name="TTo"> Type to map to </typeparam>
        /// <param name="action"> Mapping </param>
        /// <returns> This container </returns>
        public MapperContainer Register<TFrom, TTo>(
            Action<TFrom, TTo, MapperContainer> action)
        {
            Register(GetKey<TFrom, TTo>(), action);

            return this;
        }

        /// <summary>
        ///   <para> Check the mapper exists in this container </para>
        /// </summary>
        /// <typeparam name="TFrom"> Type to map from </typeparam>
        /// <typeparam name="TTo"> Type to map to </typeparam>
        /// <returns> True if found </returns>
        public bool Contains<TFrom, TTo>()
        {
            return
                _mappers.ContainsKey(GetKey<TFrom, TTo>());
        }

        /// <summary>
        ///   <para> Get a registered mapper </para>
        /// </summary>
        /// <typeparam name="TFrom"> Type to map from </typeparam>
        /// <typeparam name="TTo"> Type to map to </typeparam>
        /// <returns> Mapper </returns>
        public Action<TFrom, TTo, MapperContainer> Get<TFrom, TTo>()
        {
            return (Action<TFrom, TTo, MapperContainer>)
                   Get(typeof (TFrom), typeof (TTo));
        }

        /// <summary>
        ///   <para> Map an object </para>
        /// </summary>
        /// <typeparam name="TFrom"> Type to map from </typeparam>
        /// <typeparam name="TTo"> Type to map to </typeparam>
        /// <param name="from"> Object to map from </param>
        /// <param name="toExpression"> Expression for the target </param>
        /// <param name="createToItem"> Create a new to item </param>
        public void Map<TFrom, TTo>(
            TFrom from,
            Expression<Func<TTo>> toExpression,
            Func<TTo> createToItem)
            where TTo : class
        {
            var toMember
                = new CollapseMembersExpressionVisitor()
                    .Modify(toExpression);
            var to = toMember.GetValue();
            if (to == null)
            {
                to = createToItem == null
                         ? (TTo) Activator.CreateInstance(toMember.Type)
                         : createToItem();
                toMember.SetValue(to);
            }

            var mapper = Get<TFrom, TTo>();

            mapper(from, to, this);
        }

        /// <summary>
        ///   <para> Map all items </para>
        /// </summary>
        /// <typeparam name="TFrom"> Type to map from </typeparam>
        /// <typeparam name="TTo"> Type to map to </typeparam>
        /// <param name="from"> Object enumerable to map from </param>
        /// <param name="toExpression"> Object collection to map to </param>
        /// <param name="match"> Match function to find objects in the 'to' items given a 'from' item </param>
        /// <param name="createToItem"> Create a new to item </param>
        public void MapAll<TFrom, TTo>(
            IEnumerable<TFrom> from,
            Expression<Func<IEnumerable<TTo>>> toExpression,
            Func<TFrom, TTo, bool> match,
            Func<TTo> createToItem)
            where TTo : class
        {
            var toMember
                = new CollapseMembersExpressionVisitor()
                    .Modify(toExpression);

            var toOriginal = toMember.GetValue();
            var toItemType = GetEnumerableElementType(toMember.Type);

            var toList = toOriginal as IList;

            if (toOriginal == null
                || toList == null
                || toList.IsFixedSize || toList.IsReadOnly)
            {
                // create a new member list when null or readonly
                toList = (IList)
                         (Implements(toMember.Type, typeof (IList))
                          && !toMember.Type.IsArray
                              ? Activator.CreateInstance(toMember.Type)
                              : Activator.CreateInstance(
                                  typeof (Collection<>).MakeGenericType(new[] {toItemType}))
                         );

                // set it to the member, knowing it is an IEnumerable<TTo>
                toMember.SetValue((IEnumerable<TTo>) toList);
            }

            // get original 'to' items for matching, and clear from member list
            var toOriginalArray = toOriginal == null
                                      ? new TTo[] {}
                                      : toOriginal.ToArray();
            toList.Clear();

            // prepare to map each 'from' item
            var fromItemType = typeof (TFrom);
            var mapper = Get(fromItemType, toItemType);
            var mapperMethod = mapper.GetType().GetMethod("Invoke");

            foreach (var fromItem in from)
            {
                // find the 'to' item or create one
                var toItem = default(TTo);
                if (match != null)
                {
                    toItem = toOriginalArray
                        .SingleOrDefault(t => match(fromItem, t));
                }
                if (toItem == null)
                {
                    toItem = createToItem == null
                                 ? (TTo) Activator.CreateInstance(toItemType)
                                 : createToItem();
                }

                // map
                mapperMethod.Invoke(mapper,
                                    new object[] {fromItem, toItem, this});

                toList.Add(toItem);
            }
        }

        internal void Register(
            Tuple<Type, Type> key, Object mapper)
        {
            lock (LockObject)
            {
                _mappers.Add(key, mapper);
            }
        }

        object Get(Type fromType, Type toType)
        {
            var key = Tuple.Create(fromType, toType);

            if (!_mappers.ContainsKey(key))
            {
                // check other implementations, and cache if found
                // allows explicit mappers first
                var mapFound = (from k in _mappers.Keys
                                where Implements(fromType, k.Item1)
                                      && Implements(toType, k.Item2)
                                select _mappers[k])
                    .FirstOrDefault();

                if (mapFound == null)
                    throw new MapperNotRegisteredException(key);

                Register(key, mapFound);
            }

            return _mappers[key];
        }

        static Tuple<Type, Type> GetKey<TFrom, TTo>()
        {
            return GetKey(typeof (TFrom), typeof (TTo));
        }

        static Tuple<Type, Type> GetKey(Type fromType, Type toType)
        {
            return Tuple.Create(fromType, toType);
        }

        #region type helpers

        /// <summary>
        ///   <para> Check a type implements another type </para>
        /// </summary>
        static bool Implements(
            Type type, Type implementedType,
            bool checkInterfaces = true)
        {
            if (implementedType.IsAssignableFrom(type))
                return true;

            if (implementedType.IsGenericType)
            {
                if (type.IsGenericType)
                {
                    var getTypeDef = type.GetGenericTypeDefinition();
                    if (getTypeDef != type
                        && implementedType.IsAssignableFrom(getTypeDef))
                        return true;
                }

                if (type.BaseType != null
                    && type.BaseType != typeof (object)
                    && Implements(type.BaseType, implementedType, false))
                    return true;

                if (checkInterfaces
                    && type.GetInterfaces().Any(i => Implements(i, implementedType, false)))
                    return true;
            }

            return false;
        }

        static Type GetEnumerableElementType(Type type)
        {
            var ienum = FindIEnumerable(type);
            return ienum == null
                       ? type
                       : ienum.GetGenericArguments()[0];
        }

        static Type FindIEnumerable(Type type)
        {
            if (type == null || type == typeof (string))
            {
                return null;
            }

            if (type.IsArray)
            {
                return typeof (IEnumerable<>).MakeGenericType(type.GetElementType());
            }

            if (type.IsGenericType)
            {
                foreach (var arg in type.GetGenericArguments())
                {
                    var ienum = typeof (IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(type))
                    {
                        return ienum;
                    }
                }
            }

            var iType = type.GetInterfaces();
            if (iType.Length > 0)
            {
                foreach (var iface in iType)
                {
                    var ienum = FindIEnumerable(iface);
                    if (ienum != null) return ienum;
                }
            }
            if (type.BaseType != null && type.BaseType != typeof (object))
            {
                return FindIEnumerable(type.BaseType);
            }

            return null;
        }

        #endregion
    }
}