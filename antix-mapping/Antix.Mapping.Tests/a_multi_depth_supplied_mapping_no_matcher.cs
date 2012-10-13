using System.Collections.Generic;
using System.Linq;
using Antix.Mapping.TestDomain.Entities;
using Antix.Mapping.TestDomain.Models;
using Xunit;

namespace Antix.Mapping.Tests
{
    public class a_multi_depth_supplied_mapping_no_matcher
    {
        readonly IMapperContext _mapperContext;
        readonly PersonEntity _to;
        readonly Person _from;

        public a_multi_depth_supplied_mapping_no_matcher()
        {
            new List<IEntity>();
            new List<IEntity>();

            _mapperContext = new MapperContext(
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
                    )
                );

            _from = new Person
                        {
                            Name = new Name {First = "Person"},
                            Addresses = new[]
                                            {
                                                new Address {Name = "Address"}
                                            }
                        };

            _to = new PersonEntity
                      {
                          Name = new NameEntity {First = "Overwite"},
                          Addresses = new[]
                                          {
                                              new AddressEntity {Name = "Overwite"},
                                              new AddressEntity {Name = "Overwite"}
                                          }
                      };

            _mapperContext.Map(_from, () => _to);
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