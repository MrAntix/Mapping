namespace Antix.Mapping
{
    public interface IMapperContext
    {
        IMapperContainer Container { get; }

        T Create<T>();
        void Update<T>(T entity);
        void Delete<T>(T entity);
    }
}