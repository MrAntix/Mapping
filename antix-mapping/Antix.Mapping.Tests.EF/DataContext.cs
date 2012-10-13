using System.Data.Entity;
using Antix.Mapping.TestDomain.Entities;

namespace Antix.Mapping.Tests.EF
{
    public class DataContext : DbContext, IMapperContext
    {
        public DataContext(IMapperContainer container)
        {
            Container = container;
        }

        public IDbSet<PersonEntity> People
        {
            get { return Set<PersonEntity>(); }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PersonEntity>()
                .HasKey(p => p.Id)
                .HasMany(p => p.Addresses)
                .WithRequired(a => a.Person);

            modelBuilder.Entity<AddressEntity>()
                .HasKey(p => p.Id);
        }

        public IMapperContainer Container { get; private set; }

        public T Create<T>()
        {
            var set = Set(typeof (T));
            return (T) set.Add(set.Create());
        }

        public void Update<T>(T entity)
        {
            // not required for EF
        }

        public void Delete<T>(T entity)
        {
            var set = Set(typeof (T));
            set.Remove(entity);
        }
    }
}