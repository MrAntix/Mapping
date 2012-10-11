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

        readonly IDictionary<Type, object>
            _creators = new Dictionary<Type, object>();

        readonly IDictionary<Type, object>
            _updaters = new Dictionary<Type, object>();

        readonly IDictionary<Type, Action<object>>
            _deleters = new Dictionary<Type, Action<object>>();

        /// <summary>
        ///   <para> Register a mapping </para>
        /// </summary>
        /// <typeparam name="TFrom"> Type to map from </typeparam>
        /// <typeparam name="TTo"> Type to map to </typeparam>
        /// <param name="action"> Mapping </param>
        /// <returns> This container </returns>
        public IMapperContainer RegisterMapper<TFrom, TTo>(
            Action<TFrom, TTo, IMapperContainer> action)
        {
            RegisterMapper(GetMapperKey<TFrom, TTo>(), action);

            return this;
        }

        /// <summary>
        ///   <para> Get a registered mapper </para>
        /// </summary>
        /// <typeparam name="TFrom"> Type to map from </typeparam>
        /// <typeparam name="TTo"> Type to map to </typeparam>
        /// <returns> Mapper </returns>
        public Action<TFrom, TTo, IMapperContainer> GetMapper<TFrom, TTo>()
        {
            return (Action<TFrom, TTo, IMapperContainer>)
                   GetMapper(typeof (TFrom), typeof (TTo));
        }

        /// <summary>
        ///   <para> Register the creator </para>
        /// </summary>
        /// <typeparam name="T"> Type to create </typeparam>
        /// <param name="func"> Creator function </param>
        /// <returns> This Container </returns>
        public IMapperContainer RegisterCreator<T>(Func<Type, T> func)
        {
            RegisterCreator(typeof (T), func);

            return this;
        }

        /// <summary>
        ///   <para> Register the updater </para>
        /// </summary>
        /// <typeparam name="T"> Type to update </typeparam>
        /// <param name="action"> Updater function </param>
        /// <returns> This Container </returns>
        public IMapperContainer RegisterUpdater<T>(Action<T> action)
        {
            RegisterUpdater(typeof (T), action);

            return this;
        }

        /// <summary>
        ///   <para> Register the deteter </para>
        /// </summary>
        /// <typeparam name="T"> Type to detete </typeparam>
        /// <param name="action"> Deleter function </param>
        /// <returns> This Container </returns>
        public IMapperContainer RegisterDeleter<T>(Action<T> action)
        {
            RegisterDeleter(typeof (T), o => action((T) o));

            return this;
        }

        /// <summary>
        ///   <para> Check the mapper exists in this container </para>
        /// </summary>
        /// <typeparam name="TFrom"> Type to map from </typeparam>
        /// <typeparam name="TTo"> Type to map to </typeparam>
        /// <returns> True if found </returns>
        public bool ContainsMapper<TFrom, TTo>()
        {
            return
                _mappers.ContainsKey(GetMapperKey<TFrom, TTo>());
        }

        /// <summary>
        ///   <para> Map an object </para>
        /// </summary>
        /// <typeparam name="TFrom"> Type to map from </typeparam>
        /// <typeparam name="TTo"> Type to map to </typeparam>
        /// <param name="from"> Object to map from </param>
        /// <param name="toExpression"> Expression for the target </param>
        public void Map<TFrom, TTo>(
            TFrom from,
            Expression<Func<TTo>> toExpression)
            where TTo : class
        {
            if (toExpression == null) throw new ArgumentNullException("toExpression");
            if (Equals(from, default(TFrom))) return;

            var toMember
                = new CollapseMembersExpressionVisitor()
                    .Modify(toExpression);
            var to = toMember.GetValue();
            if (to == null)
            {
                var creator = GetCreator<TTo>()
                    ?? Activator.CreateInstance;

                to = (TTo) creator(toMember.Type);
                toMember.SetValue(to);
            }

            var mapper = GetMapper<TFrom, TTo>();

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
        public void MapAll<TFrom, TTo>(
            IEnumerable<TFrom> from,
            Expression<Func<IEnumerable<TTo>>> toExpression,
            Func<TFrom, TTo, bool> match)
            where TTo : class
        {
            if (toExpression == null) throw new ArgumentNullException("toExpression");
            if (from == null) return;

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
            var toListOriginal = toOriginal == null
                                     ? new List<TTo>()
                                     : toOriginal.ToList();
            toList.Clear();

            // prepare to map each 'from' item
            var fromItemType = typeof (TFrom);
            var mapper = GetMapper(fromItemType, toItemType);
            var mapperMethod = mapper.GetType().GetMethod("Invoke");

            var creator = GetCreator<TTo>()
                ?? Activator.CreateInstance;

            foreach (var fromItem in from)
            {
                // find the 'to' item or create one
                var toItem = default(TTo);
                if (match != null)
                {
                    toItem = toListOriginal
                        .SingleOrDefault(t => match(fromItem, t));

                    toListOriginal.Remove(toItem);
                }

                if (toItem == null)
                {
                    toItem = (TTo) creator(toItemType);
                }

                // map
                mapperMethod.Invoke(mapper,
                                    new object[] {fromItem, toItem, this});

                toList.Add(toItem);
            }

            if (!toListOriginal.Any()) return;

            var deleter = GetDeleter<TTo>();
            if (deleter == null) return;

            foreach (var toItem in toListOriginal) deleter(toItem);
        }

        internal void RegisterMapper(
            Tuple<Type, Type> key, Object mapper)
        {
            lock (LockObject)
            {
                _mappers.Add(key, mapper);
            }
        }

        object GetMapper(Type fromType, Type toType)
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

                RegisterMapper(key, mapFound);
            }

            return _mappers[key];
        }

        static Tuple<Type, Type> GetMapperKey<TFrom, TTo>()
        {
            return GetMapperKey(typeof (TFrom), typeof (TTo));
        }

        static Tuple<Type, Type> GetMapperKey(Type fromType, Type toType)
        {
            return Tuple.Create(fromType, toType);
        }

        void RegisterCreator(Type type, object func)
        {
            lock (LockObject)
            {
                _creators.Add(type, func);
            }
        }

        Func<Type, object> GetCreator<T>()
        {
            var type = typeof (T);
            if (!_creators.ContainsKey(type))
            {
                // check other implementations, and cache if found
                var found = (Func<Type, object>)
                            (from k in _creators.Keys
                             where Implements(type, k)
                             select _creators[k])
                                .FirstOrDefault();

                RegisterCreator(type, found);
            }

            return (Func<Type, object>) _creators[type];
        }

        void RegisterUpdater(Type type, object func)
        {
            lock (LockObject)
            {
                _updaters.Add(type, func);
            }
        }

        void RegisterDeleter(Type type, Action<object> func)
        {
            lock (LockObject)
            {
                _deleters.Add(type, func);
            }
        }

        Action<object> GetDeleter<T>()
        {
            var type = typeof (T);
            if (!_deleters.ContainsKey(type))
            {
                // check other implementations, and cache if found
                var found = (from k in _deleters.Keys
                             where Implements(type, k)
                             select _deleters[k])
                    .FirstOrDefault();

                RegisterDeleter(type, found);
            }

            return _deleters[type];
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