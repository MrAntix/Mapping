This is a simple container for object mapping

[On NuGet](https://nuget.org/packages/antix-mapping)

When you map your objects you get the container passed into the mapper too, this means you can call other mappings from inside your mapping and keep your code all nice and DRY

### Basic Example

    _mapperContainer = new MapperContainer();

    _mapperContainer
        .Register<Person, PersonEntity>(
            (f, t, c) =>
                {
                    f.Name = t.Name;
                    c.MapAll(f.Addresses, () => t.Addresses);
                })
        .Register<Address, AddressEntity>(
            (f, t, c) => { t.Name = f.Name; }
        );

In this example the registered Address => AddressEnity mapper is called for the collection of Addresses on the person

See the tests for more examples