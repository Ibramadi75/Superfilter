using Database.Models;
using Superfilter;
using Superfilter.Constants;
using Superfilter.Entities;
using Tests.Common;

namespace Tests.Unit;

public class NestedPropertyFilteringTests
{
    private static IQueryable<User> GetTestUsers()
    {
        Brand brandFord = new() { Id = 1, Name = "Ford", Rate = 5 };
        Brand brandFiat = new() { Id = 2, Name = "Fiat", Rate = 3 };
        Brand brandBmw = new() { Id = 3, Name = "BMW", Rate = 4 };
        Brand brandHonda = new() { Id = 4, Name = "Honda", Rate = 2 };

        User mayor = new()
        {
            Id = 999, Name = "SuperMayor", MoneyAmount = 150,
            Car = new Car { Id = 1, Name = "Ford Fiesta", Brand = brandFiat },
            House = new House { Id = 1, Address = "123 Main Street" }
        };
        City paris = new() { Id = 1, Name = "Paris", Mayor = mayor, MayorId = mayor.Id };
        City chatillon = new() { Id = 2, Name = "Chatillon" };

        return new List<User>
        {
            new()
            {
                Id = 1, Name = "Alice", MoneyAmount = 150,
                Car = new Car { Id = 1, Name = "Ford Fiesta", Brand = brandFord },
                House = new House { Id = 1, Address = "123 Main Street", City = paris, CityId = 2 }
            },
            new()
            {
                Id = 2, Name = "Bob", MoneyAmount = 200,
                Car = new Car { Id = 2, Name = "Fiat 500", Brand = brandFiat },
                House = new House { Id = 2, Address = "456 Oak Street", City = chatillon, CityId = 2 }
            },
            new()
            {
                Id = 3, Name = "Charlie", MoneyAmount = 50,
                Car = new Car { Id = 3, Name = "BMW X3", Brand = brandBmw },
                House = new House { Id = 3, Address = "789 Pine Avenue" }
            },
            new()
            {
                Id = 4, Name = "Dave", MoneyAmount = 300,
                Car = new Car { Id = 4, Name = "Honda Civic", Brand = brandHonda },
                House = new House { Id = 4, Address = "101 Maple Road" }
            },
            mayor
        }.AsQueryable();
    }

    [Fact]
    public void FilterByCarBrandName_FiltersCorrectly()
    {
        IQueryable<User> users = GetTestUsers();
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("carBrandName", Operator.Equals, "BMW")]
        };

        var superfilter = SuperfilterBuilder.For<User>()
            .MapProperty("carBrandName", x => x.Car!.Brand!.Name)
            .WithFilters(filters)
            .Build();

        List<User> result = superfilter.ApplyConfiguredFilters(users).ToList();

        Assert.Single(result);
        Assert.Contains(result, user => user.Car!.Brand!.Name == "BMW");
    }

    [Fact]
    public void FilterByHouseAddress_FiltersCorrectly()
    {
        IQueryable<User> users = GetTestUsers();
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("houseAddress", Operator.Contains, "Oak")]
        };

        var superfilter = SuperfilterBuilder.For<User>()
            .MapProperty("id", x => x.Id)
            .MapProperty("houseAddress", x => x.House!.Address)
            .WithFilters(filters)
            .Build();

        List<User> result = superfilter.ApplyConfiguredFilters(users).ToList();

        Assert.Single(result);
        Assert.Contains(result, user => user.House!.Address.Contains("Oak"));
    }

    [Fact]
    public void FilterByCarBrandRate_FiltersCorrectly()
    {
        IQueryable<User> users = GetTestUsers();
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("carBrandRate", Operator.GreaterThan, "3")]
        };

        var superfilter = SuperfilterBuilder.For<User>()
            .MapProperty("carBrandRate", x => x.Car!.Brand!.Rate)
            .WithFilters(filters)
            .Build();

        List<User> result = superfilter.ApplyConfiguredFilters(users).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, user => user.Car!.Brand!.Rate == 5);
        Assert.Contains(result, user => user.Car!.Brand!.Rate == 4);
    }

    [Fact]
    public void FilterByDoubleNestedProperty_FiltersCorrectly()
    {
        IQueryable<User> users = GetTestUsers();
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("carBrandName", Operator.Equals, "Ford")]
        };

        var superfilter = SuperfilterBuilder.For<User>()
            .MapProperty("carBrandName", x => x.Car!.Brand!.Name)
            .WithFilters(filters)
            .Build();

        List<User> result = superfilter.ApplyConfiguredFilters(users).ToList();

        Assert.Single(result);
        Assert.Contains(result, user => user.Car!.Brand!.Name == "Ford");
    }
}