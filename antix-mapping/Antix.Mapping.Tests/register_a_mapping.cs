using Antix.Mapping.Properties;
using Antix.Mapping.Tests.Entities;
using Antix.Mapping.Tests.Models;
using Xunit;

namespace Antix.Mapping.Tests
{
    public class register_a_mapping
    {
        readonly MapperContainer _mapperContainer;

        public register_a_mapping()
        {
            _mapperContainer = new MapperContainer();

            _mapperContainer
                .Register<Person, PersonEntity>(
                    (f, t, c) => { }
                );
        }

        [Fact]
        void map_exists()
        {
            Assert.True(
                _mapperContainer
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
                () => _mapperContainer.Map(new object(), () => result)
                );

            Assert.Equal(
                expectedMessage,
                ex.Message
                );
        }
    }
}