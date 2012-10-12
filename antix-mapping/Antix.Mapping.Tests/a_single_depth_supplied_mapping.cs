using Antix.Mapping.Tests.Entities;
using Antix.Mapping.Tests.Models;
using Xunit;

namespace Antix.Mapping.Tests
{
    public class a_single_depth_supplied_mapping
    {
        readonly IMapperContext _mapperContext;

        public a_single_depth_supplied_mapping()
        {
            _mapperContext = new MapperContext(
                new MapperContainer()
                    .Register<Person, PersonEntity>(
                        (f, t, c) => { t.Email = f.Email; }
                    )
                );
        }

        [Fact]
        void maps_correctly()
        {
            var from = new Person
                           {
                               Email = "test@example.com"
                           };

            var to = new PersonEntity();

            _mapperContext.Map(from, () => to);

            Assert.Equal(from.Email, to.Email);
        }
    }
}