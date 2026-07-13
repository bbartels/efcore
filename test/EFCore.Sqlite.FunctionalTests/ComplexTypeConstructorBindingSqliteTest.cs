// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

public class ComplexTypeConstructorBindingSqliteTest
{
    [Fact]
    public async Task Complex_property_is_injected_into_constructor_when_querying()
    {
        await using var testStore = SqliteTestStore.Create("ComplexCtorBindingQuery");

        using (var context = new CustomerContext(testStore))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreatedResiliently();
            context.Add(
                new Customer(new Address("Main St", "Springfield")) { Id = 1, Name = "Homer" });
            context.SaveChanges();
        }

        using (var context = new CustomerContext(testStore))
        {
            var customer = context.Set<Customer>().Single();

            Assert.Equal(1, customer.Id);
            Assert.Equal("Homer", customer.Name);
            Assert.Equal("Main St", customer.Address.Street);
            Assert.Equal("Springfield", customer.Address.City);
        }
    }

    [Fact]
    public async Task Complex_property_is_injected_into_constructor_when_querying_with_no_tracking()
    {
        await using var testStore = SqliteTestStore.Create("ComplexCtorBindingNoTracking");

        using (var context = new CustomerContext(testStore))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreatedResiliently();
            context.Add(
                new Customer(new Address("Main St", "Springfield")) { Id = 1, Name = "Homer" });
            context.SaveChanges();
        }

        using (var context = new CustomerContext(testStore))
        {
            var customer = context.Set<Customer>().AsNoTracking().Single();

            Assert.Equal("Main St", customer.Address.Street);
            Assert.Equal("Springfield", customer.Address.City);
        }
    }

    [Fact]
    public async Task Complex_property_injected_into_constructor_can_be_change_tracked()
    {
        await using var testStore = SqliteTestStore.Create("ComplexCtorBindingTracking");

        using (var context = new CustomerContext(testStore))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreatedResiliently();
            context.Add(
                new Customer(new Address("Main St", "Springfield")) { Id = 1, Name = "Homer" });
            context.SaveChanges();
        }

        using (var context = new CustomerContext(testStore))
        {
            var customer = context.Set<Customer>().Single();
            customer.Address.Street = "Evergreen Terrace";
            context.SaveChanges();
        }

        using (var context = new CustomerContext(testStore))
        {
            var customer = context.Set<Customer>().Single();

            Assert.Equal("Evergreen Terrace", customer.Address.Street);
            Assert.Equal("Springfield", customer.Address.City);
        }
    }

    [Fact]
    public async Task Complex_property_is_injected_into_constructor_in_projection()
    {
        await using var testStore = SqliteTestStore.Create("ComplexCtorBindingProjection");

        using (var context = new CustomerContext(testStore))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreatedResiliently();
            context.Add(
                new Customer(new Address("Main St", "Springfield")) { Id = 1, Name = "Homer" });
            context.SaveChanges();
        }

        using (var context = new CustomerContext(testStore))
        {
            var address = context.Set<Customer>().Select(c => c.Address).Single();

            Assert.Equal("Main St", address.Street);
            Assert.Equal("Springfield", address.City);
        }
    }

    [Fact]
    public async Task Nested_complex_properties_are_injected_into_constructors_when_querying()
    {
        await using var testStore = SqliteTestStore.Create("ComplexCtorBindingNested");

        using (var context = new WarehouseContext(testStore))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreatedResiliently();
            context.Add(
                new Warehouse(new Location("Docks", new GeoCoordinate(47.6, -122.3))) { Id = 1 });
            context.SaveChanges();
        }

        using (var context = new WarehouseContext(testStore))
        {
            var warehouse = context.Set<Warehouse>().Single();

            Assert.Equal(1, warehouse.Id);
            Assert.Equal("Docks", warehouse.Location.Name);
            Assert.Equal(47.6, warehouse.Location.Coordinate.Latitude);
            Assert.Equal(-122.3, warehouse.Location.Coordinate.Longitude);
        }
    }

    [Fact]
    public async Task Readonly_record_struct_complex_property_is_injected_into_constructor_when_querying()
    {
        await using var testStore = SqliteTestStore.Create("ComplexCtorBindingRecordStruct");

        using (var context = new RecordStructCustomerContext(testStore))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreatedResiliently();
            context.Add(
                new RecordStructCustomer(new RecordStructAddress("Main St", "Springfield")) { Id = 1 });
            context.SaveChanges();
        }

        using (var context = new RecordStructCustomerContext(testStore))
        {
            var customer = context.Set<RecordStructCustomer>().Single();

            Assert.Equal(1, customer.Id);
            Assert.Equal("Main St", customer.Address.Street);
            Assert.Equal("Springfield", customer.Address.City);
        }
    }

    [Fact]
    public async Task Nullable_complex_property_injected_into_constructor_materializes_null()
    {
        await using var testStore = SqliteTestStore.Create("ComplexCtorBindingNullable");

        using (var context = new OptionalCustomerContext(testStore))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreatedResiliently();
            context.Add(new OptionalCustomer(null) { Id = 1 });
            context.Add(new OptionalCustomer(new Address("Main St", "Springfield")) { Id = 2 });
            context.SaveChanges();
        }

        using (var context = new OptionalCustomerContext(testStore))
        {
            var customers = context.Set<OptionalCustomer>().OrderBy(e => e.Id).ToList();

            Assert.Null(customers[0].Address);
            Assert.Equal("Main St", customers[1].Address!.Street);
            Assert.Equal("Springfield", customers[1].Address!.City);
        }
    }

    [Fact]
    public async Task Throws_for_complex_property_mapped_to_json_injected_into_constructor()
    {
        await using var testStore = SqliteTestStore.Create("ComplexCtorBindingJson");

        using (var context = new JsonCustomerContext(testStore))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreatedResiliently();
            context.Add(
                new JsonCustomer(new Address("Main St", "Springfield")) { Id = 1 });
            context.SaveChanges();
        }

        using (var context = new JsonCustomerContext(testStore))
        {
            Assert.Equal(
                CoreStrings.ComplexPropertyConstructorBindingNotSupported(
                    nameof(JsonCustomer), nameof(JsonCustomer.Address)),
                Assert.Throws<InvalidOperationException>(
                    () => context.Set<JsonCustomer>().Single()).Message);
        }
    }

    [Fact]
    public async Task Json_complex_property_uses_parameterless_fallback_constructor()
    {
        await using var testStore = SqliteTestStore.Create("ComplexCtorBindingJsonFallback");

        using (var context = new JsonFallbackCustomerContext(testStore))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreatedResiliently();
            context.Add(
                new JsonFallbackCustomer(new Address("Main St", "Springfield")) { Id = 1 });
            context.SaveChanges();
        }

        using (var context = new JsonFallbackCustomerContext(testStore))
        {
            var customer = context.Set<JsonFallbackCustomer>().Single();

            Assert.True(customer.ParameterlessConstructorUsed);
            Assert.Equal("Main St", customer.Address.Street);
            Assert.Equal("Springfield", customer.Address.City);
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Complex_collection_is_injected_into_get_only_constructor_property_when_querying(
        bool noTracking,
        bool empty)
    {
        await using var testStore = SqliteTestStore.Create($"ComplexCollectionCtorBindingQuery{noTracking}{empty}");

        using (var context = new CustomerWithAddressCollectionContext(testStore))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreatedResiliently();
            context.Add(
                new CustomerWithAddressCollection(
                    empty
                        ? []
                        :
                        [
                            new Address("Main St", "Springfield"),
                            new Address("Evergreen Terrace", "Springfield")
                        ])
                {
                    Id = 1
                });
            context.SaveChanges();
        }

        using (var context = new CustomerWithAddressCollectionContext(testStore))
        {
            var entityType = context.Model.FindEntityType(typeof(CustomerWithAddressCollection))!;
            var binding = Assert.IsType<ComplexPropertyParameterBinding>(
                ((IEntityType)entityType).ConstructorBinding!.ParameterBindings.Single());
            Assert.Same(
                entityType.FindComplexProperty(nameof(CustomerWithAddressCollection.Addresses)),
                binding.ConsumedProperties.Single());

            var query = context.Set<CustomerWithAddressCollection>().AsQueryable();
            var customer = (noTracking ? query.AsNoTracking() : query).Single();

            Assert.Equal(1, customer.Id);
            Assert.Equal(empty ? 0 : 2, customer.ConstructorAddressCount);
            Assert.Equal(
                empty ? [] : ["Main St", "Evergreen Terrace"],
                customer.ConstructorAddressSnapshot);
            if (empty)
            {
                Assert.Empty(customer.Addresses);
            }
            else
            {
                Assert.Collection(
                    customer.Addresses,
                    address =>
                    {
                        Assert.Equal("Main St", address.Street);
                        Assert.Equal("Springfield", address.City);
                    },
                    address =>
                    {
                        Assert.Equal("Evergreen Terrace", address.Street);
                        Assert.Equal("Springfield", address.City);
                    });
            }
        }
    }

    [Fact]
    public async Task Complex_collection_injected_through_constructor_is_tracked_and_updated()
    {
        await using var testStore = SqliteTestStore.Create("ComplexCollectionCtorBindingTracking");

        using (var context = new CustomerWithAddressCollectionContext(testStore))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreatedResiliently();
            context.Add(
                new CustomerWithAddressCollection(
                    [
                        new Address("Main St", "Springfield"),
                        new Address("Evergreen Terrace", "Springfield")
                    ])
                {
                    Id = 1
                });
            context.SaveChanges();
        }

        using (var context = new CustomerWithAddressCollectionContext(testStore))
        {
            var customer = context.Set<CustomerWithAddressCollection>().Single();
            var collectionEntry = context.Entry(customer).ComplexCollection(nameof(CustomerWithAddressCollection.Addresses));

            Assert.Equal("Main St", collectionEntry[0].Property(nameof(Address.Street)).OriginalValue);
            Assert.Equal("Evergreen Terrace", collectionEntry[1].Property(nameof(Address.Street)).OriginalValue);

            customer.Addresses.Reverse();
            customer.Addresses[0].Street = "Changed Terrace";

            context.ChangeTracker.DetectChanges();

            Assert.True(collectionEntry.IsModified);
            Assert.Equal(1, context.SaveChanges());
        }

        using (var context = new CustomerWithAddressCollectionContext(testStore))
        {
            var customer = context.Set<CustomerWithAddressCollection>().AsNoTracking().Single();

            Assert.Collection(
                customer.Addresses,
                address => Assert.Equal("Changed Terrace", address.Street),
                address => Assert.Equal("Main St", address.Street));
        }
    }

    [Fact]
    public async Task Complex_collection_is_injected_through_proxy_constructor_binding()
    {
        await using var testStore = SqliteTestStore.Create("ComplexCollectionCtorBindingProxy");

        using (var context = new ProxiedCustomerWithAddressCollectionContext(testStore))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreatedResiliently();
            context.Add(
                new ProxiedCustomerWithAddressCollection(
                    [new ProxyAddress("Main St", "Springfield")])
                {
                    Id = 1
                });
            context.SaveChanges();
        }

        using (var context = new ProxiedCustomerWithAddressCollectionContext(testStore))
        {
            var customer = context.Set<ProxiedCustomerWithAddressCollection>().Single();

            Assert.NotEqual(typeof(ProxiedCustomerWithAddressCollection), customer.GetType());
            Assert.Equal(1, customer.ConstructorAddressCount);
            Assert.Equal("Main St", Assert.Single(customer.Addresses).Street);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Null_complex_collection_is_injected_as_null_when_querying(bool noTracking)
    {
        await using var testStore = SqliteTestStore.Create($"NullComplexCollectionCtorBindingQuery{noTracking}");

        using (var context = new CustomerWithNullableAddressCollectionContext(testStore))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreatedResiliently();
            context.Add(new CustomerWithNullableAddressCollection(null) { Id = 1 });
            context.SaveChanges();
        }

        using (var context = new CustomerWithNullableAddressCollectionContext(testStore))
        {
            var query = context.Set<CustomerWithNullableAddressCollection>().AsQueryable();
            var customer = (noTracking ? query.AsNoTracking() : query).Single();

            Assert.True(customer.ConstructorReceivedNull);
            Assert.Null(customer.Addresses);
        }
    }

    [Fact]
    public async Task Nested_complex_collection_is_injected_into_complex_type_constructor_when_querying()
    {
        await using var testStore = SqliteTestStore.Create("NestedComplexCollectionCtorBindingQuery");

        using (var context = new CustomerWithProfileContext(testStore))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreatedResiliently();
            context.Add(
                new CustomerWithProfile
                {
                    Id = 1,
                    Profile = new Profile(
                        "Primary",
                        [
                            new Address("Main St", "Springfield"),
                            new Address("Evergreen Terrace", "Springfield")
                        ])
                });
            context.SaveChanges();
        }

        using (var context = new CustomerWithProfileContext(testStore))
        {
            var customer = context.Set<CustomerWithProfile>().Single();

            Assert.Equal("Primary", customer.Profile.Name);
            Assert.Equal(2, customer.Profile.ConstructorAddressCount);
            Assert.Collection(
                customer.Profile.Addresses,
                address => Assert.Equal("Main St", address.Street),
                address => Assert.Equal("Evergreen Terrace", address.Street));
        }
    }

    private class CustomerContext(SqliteTestStore testStore) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(testStore.ConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Customer>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.ComplexProperty(e => e.Address);
                });
    }

    private class Customer(Address address)
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public Address Address { get; } = address;
    }

    private class Address(string street, string city)
    {
        public string Street { get; set; } = street;
        public string City { get; set; } = city;
    }

    private class WarehouseContext(SqliteTestStore testStore) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(testStore.ConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Warehouse>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.ComplexProperty(e => e.Location, lb => lb.ComplexProperty(l => l.Coordinate));
                });
    }

    private class Warehouse(Location location)
    {
        public int Id { get; set; }
        public Location Location { get; } = location;
    }

    private class Location(string name, GeoCoordinate coordinate)
    {
        public string Name { get; set; } = name;
        public GeoCoordinate Coordinate { get; } = coordinate;
    }

    private class GeoCoordinate(double latitude, double longitude)
    {
        public double Latitude { get; set; } = latitude;
        public double Longitude { get; set; } = longitude;
    }

    private class RecordStructCustomerContext(SqliteTestStore testStore) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(testStore.ConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<RecordStructCustomer>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.ComplexProperty(e => e.Address);
                });
    }

    private class RecordStructCustomer(RecordStructAddress address)
    {
        public int Id { get; set; }
        public RecordStructAddress Address { get; } = address;
    }

    private readonly record struct RecordStructAddress(string Street, string City);

    private class OptionalCustomerContext(SqliteTestStore testStore) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(testStore.ConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<OptionalCustomer>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.ComplexProperty(e => e.Address).IsRequired(false);
                });
    }

    private class OptionalCustomer(Address? address)
    {
        public int Id { get; set; }
        public Address? Address { get; set; } = address;
    }

    private class JsonCustomerContext(SqliteTestStore testStore) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(testStore.ConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<JsonCustomer>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.ComplexProperty(e => e.Address, cb => cb.ToJson());
                });
    }

    private class JsonCustomer(Address address)
    {
        public int Id { get; set; }
        public Address Address { get; set; } = address;
    }

    private class JsonFallbackCustomerContext(SqliteTestStore testStore) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(testStore.ConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<JsonFallbackCustomer>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Ignore(e => e.ParameterlessConstructorUsed);
                    b.ComplexProperty(e => e.Address, cb => cb.ToJson());
                });
    }

    private class JsonFallbackCustomer
    {
        private JsonFallbackCustomer()
            => ParameterlessConstructorUsed = true;

        public JsonFallbackCustomer(Address address)
            => Address = address;

        public int Id { get; set; }
        public Address Address { get; set; } = null!;
        public bool ParameterlessConstructorUsed { get; }
    }

    private class CustomerWithAddressCollectionContext(SqliteTestStore testStore) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(testStore.ConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<CustomerWithAddressCollection>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Ignore(e => e.ConstructorAddressCount);
                    b.Ignore(e => e.ConstructorAddressSnapshot);
                    b.ComplexCollection(e => e.Addresses, cb => cb.ToJson());
                });
    }

    private class CustomerWithAddressCollection(List<Address> addresses)
    {
        public int Id { get; set; }
        public List<Address> Addresses { get; } = addresses;
        public int ConstructorAddressCount { get; } = addresses.Count;
        public string[] ConstructorAddressSnapshot { get; } = addresses.Select(a => a.Street).ToArray();
    }

    private class ProxiedCustomerWithAddressCollectionContext(SqliteTestStore testStore) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseLazyLoadingProxies()
                .UseSqlite(testStore.ConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<ProxiedCustomerWithAddressCollection>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Ignore(e => e.ConstructorAddressCount);
                    b.ComplexCollection(e => e.Addresses, cb => cb.ToJson());
                });
    }

    public class ProxiedCustomerWithAddressCollection
    {
        public ProxiedCustomerWithAddressCollection(List<ProxyAddress> addresses)
        {
            Addresses = addresses;
            ConstructorAddressCount = addresses.Count;
        }

        public virtual int Id { get; set; }
        public virtual List<ProxyAddress> Addresses { get; }
        public int ConstructorAddressCount { get; }
    }

    public class ProxyAddress(string street, string city)
    {
        public string Street { get; set; } = street;
        public string City { get; set; } = city;
    }

    private class CustomerWithNullableAddressCollectionContext(SqliteTestStore testStore) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(testStore.ConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<CustomerWithNullableAddressCollection>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Ignore(e => e.ConstructorReceivedNull);
                    b.ComplexCollection(e => e.Addresses, cb => cb.ToJson());
                });
    }

    private class CustomerWithNullableAddressCollection(List<Address>? addresses)
    {
        public int Id { get; set; }
        public List<Address>? Addresses { get; } = addresses;
        public bool ConstructorReceivedNull { get; } = addresses is null;
    }

    private class CustomerWithProfileContext(SqliteTestStore testStore) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(testStore.ConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<CustomerWithProfile>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.ComplexProperty(
                        e => e.Profile,
                        pb =>
                        {
                            pb.ToJson();
                            pb.Property(p => p.Name);
                            pb.Ignore(p => p.ConstructorAddressCount);
                            pb.ComplexCollection(p => p.Addresses);
                        });
                });
    }

    private class CustomerWithProfile
    {
        public int Id { get; set; }
        public Profile Profile { get; set; } = null!;
    }

    private class Profile(string name, List<Address> addresses)
    {
        public string Name { get; } = name;
        public List<Address> Addresses { get; } = addresses;
        public int ConstructorAddressCount { get; } = addresses.Count;
    }
}
