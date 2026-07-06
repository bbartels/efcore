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
            context.Database.EnsureCreatedResiliently();
            context.Add(
                new Customer(new Address("Main St", "Springfield")) { Id = 1, Name = "Homer" });
            context.SaveChanges();
        }

        using (var context = new CustomerContext(testStore))
        {
            var customer = context.Set<Customer>().Single();
            customer.Address = new Address("Evergreen Terrace", "Springfield");
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
    public async Task Nullable_complex_property_injected_into_constructor_materializes_null()
    {
        await using var testStore = SqliteTestStore.Create("ComplexCtorBindingNullable");

        using (var context = new OptionalCustomerContext(testStore))
        {
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
        public Address Address { get; set; } = address;
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
}
