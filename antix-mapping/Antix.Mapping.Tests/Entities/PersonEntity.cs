using System.Collections.Generic;

namespace Antix.Mapping.Tests.Entities
{
    public class NameEntity
    {
        public string First { get; set; }
        public string Last { get; set; }
    }

    public class PersonEntity
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