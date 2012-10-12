using System;

namespace Antix.Mapping
{
    public class MapperContext : IMapperContext
    {
        readonly IMapperContainer _container;
        readonly Func<Type, object> _creator;
        readonly Action<object> _updater;
        readonly Action<object> _deleter;

        public MapperContext(
            IMapperContainer container) :
                this(container, new Parameters())
        {
        }

        public MapperContext(
            IMapperContainer container,
            Parameters parameters)
        {
            _container = container;
            _creator = parameters.Creator ?? Activator.CreateInstance;
            _updater = parameters.Updater ?? (i => { });
            _deleter = parameters.Deleter ?? (i => { });
        }

        public class Parameters
        {
            public Func<Type, object> Creator { get; set; }
            public Action<object> Updater { get; set; }
            public Action<object> Deleter { get; set; }
        }

        public IMapperContainer Container
        {
            get { return _container; }
        }

        public T Create<T>()
        {
            return (T) _creator(typeof (T));
        }

        public void Update<T>(T entity)
        {
            _updater(entity);
        }

        public void Delete<T>(T entity)
        {
            _deleter(entity);
        }
    }
}