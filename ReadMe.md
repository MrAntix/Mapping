This is a simple container for object mapping

[On NuGet](https://nuget.org/packages/antix-mapping)

When you map your objects you get the container passed into the mapper too, this means you can call other mappings from inside your mapping and keep your code all nice and DRY

Here are a couple of simple examples, *See the tests for more*

### Basic Example

Given these simple classes which need mapping, model=>entity
	public class PersonModel
    {
        public string Name { get; set; }
        public ICollection<AddressModel> Addresses { get; set; }
    }

	public class AddressModel
    {
        public string Name { get; set; }
    }

	public class PersonEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<AddressEntity> Addresses { get; set; }
    }

	public class AddressEntity : IEntity
    {
        public string Name { get; set; }
    }

Set up the container in your composite root as a single instance

    var mapperContainer = new MapperContainer();

    var mapperContainer
        .Register<PersonModel, PersonEntity>(
            (model, entity, context) =>
                {
                    entity.Name = model.Name;
                    c.MapAll(model.Addresses, () => entity.Addresses);
                })
        .Register<AddressModel, AddressEntity>(
            (model, entity, context) => 
				{
					entity.Name = model.Name; 
				}
        );

Create a new context as required like so, this can all be hooked by your IoC container

	var mapperContext = new MapperContext(mapperContainer);

Inject the mapping context where you need it

	public class PersonController : Controller 
	{
		IMapperContext _mapperContext;

		public PersonController(IMapperContext mapperContext)
		{
			_mapperContext = mapperContext;
		}

		ActionResult Update(PersonModel model) {

			var entity = entityService.Load([SomeCriteria]);

			_mapperContext.Map(model, () => entity);

			entityService.Save(entity);
		}
	}

In this example the registered AddressModel => AddressEnity mapper is called for the collection of Addresses on the person
Both Person and Address sub-collection objects are mapped

### EntityFramework Example

You can use the mapper in entity framework by making your DbContext derived class also implement IMappingContext
In this example the IMapperContainer is injected in to the DataContext object

    public class DataContext : DbContext, IMapperContext
    {
        public DataContext(IMapperContainer container)
        {
            Container = container;
        }

        public IMapperContainer Container { get; private set; }

        public T Create<T>()
        {
			// this function is called by the mapper when 
			// a new entity is needed
            var set = Set(typeof (T));
            return (T) set.Add(set.Create());
        }

        public void Update<T>(T entity)
        {
            // not required for EF
        }

        public void Delete<T>(T entity)
        {
			// this function is called by the mapper when 
			// an entity is deleted
            var set = Set(typeof (T));
            set.Remove(entity);
        }

		... and other DbContext type stuff 
				IDbSet<PersonEntity> People etc
    }

Now you can use the mapping extensions right on your data context

	var dataContext = new DataContext(mappingContainer);

	dataContext.Map(model, () => entity);