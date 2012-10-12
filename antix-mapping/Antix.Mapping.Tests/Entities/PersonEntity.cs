using System.Collections.Generic;

namespace Antix.Mapping.Tests.Entities
{
    public class PersonEntity : IEntity
    {
        public int Id { get; set; }

        public string Key { get; set; }

        public NameEntity Name { get; set; }

        public string Email { get; set; }

        public GenderEntities? Gender { get; set; }

        public RoleEntities Role { get; set; }

        public ICollection<AddressEntity> Addresses { get; set; }
    }
}