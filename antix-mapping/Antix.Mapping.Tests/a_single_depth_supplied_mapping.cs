using Antix.Mapping.Tests.Entities;
using Antix.Mapping.Tests.Models;
using Xunit;

namespace Antix.Mapping.Tests
{
    public class a_single_depth_supplied_mapping
    {
        readonly MapperContainer _mapperContainer;

        public a_single_depth_supplied_mapping()
        {
            _mapperContainer = new MapperContainer();

            _mapperContainer
                .Register<Person, PersonEntity>(
                    (f, t, c) => { t.Email = f.Email; }
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

            _mapperContainer.Map(from, () => to);

            Assert.Equal(from.Email, to.Email);
        }
    }
}