using System.Collections.Generic;

namespace Antix.Mapping.TestDomain.Models
{
    public class Person : IPerson
    {
        public Name Name { get; set; }

        IName IPerson.Name
        {
            get { return Name; }
        }

        public string Email { get; set; }
        public Genders? Gender { get; set; }
        public Roles Role { get; set; }

        public ICollection<Address> Addresses { get; set; }

        IEnumerable<IAddress> IPerson.Addresses
        {
            get { return Addresses; }
        }
    }
}