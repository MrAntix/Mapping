using System;
using Antix.Mapping.Tests.Entities;
using Antix.Mapping.Tests.Models;
using Xunit;

namespace Antix.Mapping.Tests
{
    public class calling_map
    {
        readonly MapperContainer _mapperContainer;

        public calling_map()
        {
            _mapperContainer = new MapperContainer();

            _mapperContainer
                .RegisterMapper<Person, PersonEntity>(
                    (f, t, c) => { t.Name = new NameEntity(); }
                );
        }

        [Fact]
        void no_work_done_if_from_is_null()
        {
            var entity = default(PersonEntity);

            _mapperContainer.Map(default(Person), () => entity);

            Assert.Null(entity);
        }

        [Fact]
        void work_done_if_from_is_not_null()
        {
            var entity = default(PersonEntity);

            _mapperContainer.Map(new Person(), () => entity);

            Assert.NotNull(entity);
        }

        [Fact]
        void to_expression_must_be_supplied()
        {
            Assert.Throws<ArgumentNullException>(
                () => _mapperContainer.Map<Person, PersonEntity>(null, null));
        }
    }
}