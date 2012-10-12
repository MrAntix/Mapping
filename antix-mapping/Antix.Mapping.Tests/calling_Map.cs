using System;
using Antix.Mapping.Tests.Entities;
using Antix.Mapping.Tests.Models;
using Xunit;

namespace Antix.Mapping.Tests
{
    public class calling_map
    {
        readonly IMapperContext _mapperContext;

        public calling_map()
        {
            _mapperContext = new MapperContext(
                new MapperContainer()
                    .Register<Person, PersonEntity>(
                        (f, t, c) => { t.Name = new NameEntity(); }
                    )
                );
        }

        [Fact]
        void no_work_done_if_from_is_null()
        {
            var entity = default(PersonEntity);

            _mapperContext.Map(default(Person), () => entity);

            Assert.Null(entity);
        }

        [Fact]
        void work_done_if_from_is_not_null()
        {
            var entity = default(PersonEntity);

            _mapperContext.Map(new Person(), () => entity);

            Assert.NotNull(entity);
        }

        [Fact]
        void to_expression_must_be_supplied()
        {
            Assert.Throws<ArgumentNullException>(
                () => _mapperContext.Map<Person, PersonEntity>(null, null));
        }
    }
}