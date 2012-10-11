namespace Antix.Mapping.Tests.Entities
{
    public class AddressEntity : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PersonEntity Person { get; set; }
    }
}