using System.Collections.Generic;

namespace Antix.Mapping.Tests.Models
{
    public interface IName
    {
        string First { get; set; }
        string Last { get; set; }
    }

    public interface IPerson
    {
        IName Name { get; }
        string Email { get; }
        Genders? Gender { get; }
        Roles Role { get; }

        IEnumerable<IAddress> Addresses { get; }
    }
}