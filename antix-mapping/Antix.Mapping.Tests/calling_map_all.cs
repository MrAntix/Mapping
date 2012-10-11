using System;
using System.Collections.Generic;
using Antix.Mapping.Tests.Entities;
using Antix.Mapping.Tests.Models;
using Xunit;

namespace Antix.Mapping.Tests
{
    public class calling_map_all
    {
        readonly MapperContainer _mapperContainer;

        public calling_map_all()
        {
            _mapperContainer = new MapperContainer();

            _mapperContainer
                .RegisterMapper<Person, PersonEntity>(
                    (f, t, c) => { }
                );
        }

        [Fact]
        void no_work_done_if_from_is_null()
        {
            var entities = default(List<PersonEntity>);

            _mapperContainer.MapAll(default(IEnumerable<Person>), () => entities);

            Assert.Null(entities);
        }

        [Fact]
        void work_done_if_from_is_not_null()
        {
            var entities = default(List<PersonEntity>);

            _mapperContainer.MapAll(new[]
                                        {
                                            new Person()
                                        }, () => entities);

            Assert.NotNull(entities);
        }

        [Fact]
        void to_expression_must_be_supplied()
        {
            Assert.Throws<ArgumentNullException>(
                () => _mapperContainer.MapAll<Person, PersonEntity>(null, null));
        }
    }
}