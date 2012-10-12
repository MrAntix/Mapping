using Antix.Mapping.Properties;
using Antix.Mapping.Tests.Entities;
using Antix.Mapping.Tests.Models;
using Xunit;

namespace Antix.Mapping.Tests
{
    public class register_a_mapping
    {
        readonly IMapperContext _mapperContext;

        public register_a_mapping()
        {
            _mapperContext = new MapperContext(
                new MapperContainer()
                    .Register<Person, PersonEntity>(
                        (f, t, c) => { }
                    )
                );
        }

        [Fact]
        void map_exists()
        {
            Assert.True(
                _mapperContext.Container
                    .Contains<Person, PersonEntity>()
                );
        }

        [Fact]
        void throws_where_not_registered()
        {
            var expectedMessage = string.Format(
                Resources.MapperNotRegisteredException,
                typeof (object).FullName,
                typeof (object).FullName
                );

            object result = null;

            var ex = Assert.Throws<MapperNotRegisteredException>(
                () => _mapperContext.Map(new object(), () => result)
                );

            Assert.Equal(
                expectedMessage,
                ex.Message
                );
        }
    }
}