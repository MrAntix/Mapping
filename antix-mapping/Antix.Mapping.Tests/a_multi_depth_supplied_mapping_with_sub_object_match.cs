using System;
using System.Collections.Generic;
using System.Linq;
using Antix.Mapping.Tests.Entities;
using Antix.Mapping.Tests.Models;
using Xunit;

namespace Antix.Mapping.Tests
{
    public class a_multi_depth_supplied_mapping_matcher
    {
        readonly IMapperContainer _mapperContainer;
        readonly PersonEntity _to;
        readonly Person _from;
        readonly List<IEntity> _updatedEntities;
        readonly List<IEntity> _deletedEntities;

        public a_multi_depth_supplied_mapping_matcher()
        {
            _updatedEntities = new List<IEntity>();
            _deletedEntities = new List<IEntity>();

            _mapperContainer = new MapperContainer()
                .RegisterMapper<Person, PersonEntity>(
                    (f, t, c) =>
                        {
                            c.Map(f.Name, () => t.Name);
                            c.MapAll(f.Addresses, () => t.Addresses, (fa, ta) => fa.Name == ta.Name);
                        }
                )
                .RegisterMapper<Name, NameEntity>(
                    (f, t, c) =>
                        {
                            t.First = f.First;
                            t.Last = f.Last;
                        }
                )
                .RegisterMapper<Address, AddressEntity>(
                    (f, t, c) => { t.Name = f.Name; }
                )
                .RegisterCreator(t => (IEntity) Activator.CreateInstance(t))
                .RegisterUpdater<IEntity>(e => _updatedEntities.Add(e))
                .RegisterDeleter<IEntity>(e => _deletedEntities.Add(e));

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

            _mapperContainer.Map(_from, () => _to);
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

        [Fact]
        void deletes_sub_objects()
        {
            Assert.Equal(1, _deletedEntities.Count());
            Assert.IsType<AddressEntity>(_deletedEntities.ElementAt(0));

            var address = (AddressEntity) _deletedEntities.ElementAt(0);
            Assert.Equal("Delete", address.Name);
        }
    }
}