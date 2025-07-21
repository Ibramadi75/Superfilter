using Database.Models;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Tests.Integration;

public abstract class PostgreSqlIntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("superfilter_test")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .WithCleanUp(true)
        .Build();

    protected AppDbContext Context { get; set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        Context = new AppDbContext(options);
        await Context.Database.EnsureCreatedAsync();
        await SeedTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await _container.DisposeAsync();
    }

    private async Task SeedTestDataAsync()
    {
        Brand[] brands = new[]
        {
            new Brand { Id = 1, Name = "Ford", Rate = 5 },
            new Brand { Id = 2, Name = "Fiat", Rate = 3 },
            new Brand { Id = 3, Name = "BMW", Rate = 4 },
            new Brand { Id = 4, Name = "Honda", Rate = 2 }
        };

        Car[] cars = new[]
        {
            new Car { Id = 1, Name = "Ford Fiesta", BrandId = 1, UserId = 1 },
            new Car { Id = 2, Name = "Fiat 500", BrandId = 2, UserId = 2 },
            new Car { Id = 3, Name = "BMW X3", BrandId = 3, UserId = 3 },
            new Car { Id = 4, Name = "Honda Civic", BrandId = 4, UserId = 4 }
        };

        User[] users = new[]
        {
            new User
            {
                Id = 1,
                Name = "Alice",
                MoneyAmount = 150,
                CarId = 1,
                HouseId = 1,
                BornDate = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = 2,
                Name = "Bob",
                MoneyAmount = 200,
                CarId = 2,
                HouseId = 2,
                BornDate = new DateTime(2003, 12, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = 3,
                Name = "Charlie",
                MoneyAmount = 50,
                CarId = 3,
                HouseId = 3,
                BornDate = new DateTime(1985, 8, 22, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = 4,
                Name = "Dave",
                MoneyAmount = 300,
                CarId = 4,
                HouseId = 4,
                BornDate = new DateTime(1995, 3, 7, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        City[] cities = new[]
        {
            new City { Id = 1, Name = "Paris", MayorId = 1 },
            new City { Id = 2, Name = "Chatillon", MayorId = 2 },
            new City { Id = 3, Name = "Lyon", MayorId = 3 }
        };

        House[] houses = new[]
        {
            new House { Id = 1, Address = "123 Main Street", UserId = 1, CityId = 1 },
            new House { Id = 2, Address = "456 Oak Street", UserId = 2, CityId = 2 },
            new House { Id = 3, Address = "789 Pine Avenue", UserId = 3, CityId = 3 },
            new House { Id = 4, Address = "101 Maple Road", UserId = 4, CityId = 1 }
        };

        await Context.Brands.AddRangeAsync(brands);
        await Context.SaveChangesAsync();

        await Context.Users.AddRangeAsync(users);
        await Context.SaveChangesAsync();

        await Context.Cities.AddRangeAsync(cities);
        await Context.SaveChangesAsync();

        await Context.Cars.AddRangeAsync(cars);
        await Context.SaveChangesAsync();

        await Context.Houses.AddRangeAsync(houses);
        await Context.SaveChangesAsync();
    }

    protected string GetConnectionString()
    {
        return _container.GetConnectionString();
    }
}