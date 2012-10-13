using System.Data.Entity;
using System.Linq;
using Antix.Mapping.TestDomain.Entities;
using Antix.Mapping.TestDomain.Models;
using Xunit;

namespace Antix.Mapping.Tests.EF
{
    public class when_using_ef
    {
        readonly DataContext _dataContext;
        readonly PersonEntity _to;
        readonly Person _from;

        public when_using_ef()
        {
            var mapperContainer =
                new MapperContainer()
                    .Register<Person, PersonEntity>(
                        (f, t, c) =>
                            {
                                c.Map(f.Name, () => t.Name);
                                c.MapAll(f.Addresses, () => t.Addresses);
                            })
                    .Register<Name, NameEntity>(
                        (f, t, c) =>
                            {
                                t.First = f.First;
                                t.Last = f.Last;
                            }
                    )
                    .Register<Address, AddressEntity>(
                        (f, t, c) => { t.Name = f.Name; }
                    );

            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<DataContext>());
            _dataContext = new DataContext(mapperContainer);


            _from = new Person
                        {
                            Name = new Name {First = "Person"},
                            Addresses = new[]
                                            {
                                                new Address {Name = "Keep"},
                                                new Address {Name = "New"}
                                            }
                        };

            _to = new PersonEntity
                      {
                          Name = new NameEntity {First = "Overwite"},
                          Addresses = new[]
                                          {
                                              new AddressEntity {Name = "Keep"},
                                              new AddressEntity {Name = "Delete"}
                                          }
                      };
            _to = _dataContext.Create<PersonEntity>();
            _to.Name = new NameEntity {First = "Overwite"};
            _to.Addresses = new[]
                                {
                                    new AddressEntity {Name = "Keep"},
                                    new AddressEntity {Name = "Delete"}
                                };
            _dataContext.SaveChanges();

            _dataContext.Map(_from, () => _to);
        }

        [Fact]
        void maps_parent()
        {
            Assert.Equal(_from.Name.First, _to.Name.First);
        }

        [Fact]
        void maps_parent_collection()
        {
            Assert.NotNull(_to.Addresses);
            Assert.Equal(_from.Addresses.Count, _to.Addresses.Count);
        }

        [Fact]
        void maps_sub_objects()
        {
            for (var index = 0; index < _from.Addresses.Count; index++)
            {
                Assert.Equal(
                    _from.Addresses.ElementAt(index).Name,
                    _to.Addresses.ElementAt(index).Name);
            }
        }
    }
}