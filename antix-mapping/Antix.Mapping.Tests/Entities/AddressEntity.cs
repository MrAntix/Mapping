namespace Antix.Mapping.Tests.Entities
{
    public class AddressEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PersonEntity Person { get; set; }
    }
}