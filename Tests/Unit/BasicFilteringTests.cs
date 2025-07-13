using System.Globalization;
using Database.Models;
using Superfilter.Builder;
using Superfilter.Constants;
using Superfilter.Entities;
using Tests.Common;

namespace Tests.Unit;

public class BasicFilteringTests
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
                Id = 2, Name = "Bob", MoneyAmount = 200, BornDate = DateTime.ParseExact("10/12/2003", "dd/MM/yyyy", CultureInfo.InvariantCulture),
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
    public void FilterProperty_OnInteger_ValidFilter_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("id", Operator.GreaterThan, "1")]
        };

        var config = SuperfilterBuilder.For<User>()
            .MapRequiredProperty("id", x => x.Id)
            .WithFilters(filters)
            .Build();

        Superfilter.Superfilter superfilter = new();
        superfilter.InitializeGlobalConfiguration(config);
        superfilter.InitializeFieldSelectors<User>();

        List<User> result = superfilter.ApplyConfiguredFilters(users).ToList();

        Assert.Equal(users.Count() - 1, result.Count);
        Assert.DoesNotContain(result, user => user.Id == 1);
    }

    [Fact]
    public void FilterProperty_OnDate_ValidFilter_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("bornDate", Operator.IsEqualToYear, "23/02/2003")]
        };

        var config = SuperfilterBuilder.For<User>()
            .MapProperty("bornDate", x => x.BornDate!)
            .WithFilters(filters)
            .Build();

        Superfilter.Superfilter superfilter = new();
        superfilter.InitializeGlobalConfiguration(config);
        superfilter.InitializeFieldSelectors<User>();

        List<User> result = superfilter.ApplyConfiguredFilters(users).ToList();

        Assert.Single(result);
    }

    [Fact]
    public void FilterProperty_OnString_ContainsOperator_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("name", Operator.Contains, "e")]
        };

        var config = SuperfilterBuilder.For<User>()
            .MapProperty("name", x => x.Name)
            .WithFilters(filters)
            .Build();

        Superfilter.Superfilter superfilter = new();
        superfilter.InitializeGlobalConfiguration(config);
        superfilter.InitializeFieldSelectors<User>();

        List<User> result = superfilter.ApplyConfiguredFilters(users).ToList();

        Assert.True(result.Count > 0);
        Assert.All(result, user => Assert.Contains("e", user.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void FilterProperty_NoMatchingFilter_EmptyResult()
    {
        IQueryable<User> users = GetTestUsers();
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("name", Operator.Contains, "nonexistent")]
        };

        var config = SuperfilterBuilder.For<User>()
            .MapRequiredProperty("name", x => x.Name)
            .WithFilters(filters)
            .Build();

        Superfilter.Superfilter superfilter = new();
        superfilter.InitializeGlobalConfiguration(config);
        superfilter.InitializeFieldSelectors<User>();

        List<User> result = superfilter.ApplyConfiguredFilters(users).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void FilterProperty_FieldNotInCriteria_NoFilterApplied()
    {
        IQueryable<User> users = GetTestUsers();
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("unknownField", Operator.Contains, "e")]
        };

        var config = SuperfilterBuilder.For<User>()
            .MapProperty("name", x => x.Name)
            .WithFilters(filters)
            .Build();

        Superfilter.Superfilter superfilter = new();
        superfilter.InitializeGlobalConfiguration(config);
        superfilter.InitializeFieldSelectors<User>();

        List<User> result = superfilter.ApplyConfiguredFilters(users).ToList();

        Assert.Equal(users.Count(), result.Count);
    }

    [Fact]
    public void FilterProperty_EmptyValueInFilter_SkipFiltering()
    {
        IQueryable<User> users = GetTestUsers();
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("name", Operator.Contains, "")]
        };

        var config = SuperfilterBuilder.For<User>()
            .MapProperty("name", x => x.Name)
            .WithFilters(filters)
            .Build();

        Superfilter.Superfilter superfilter = new();
        superfilter.InitializeGlobalConfiguration(config);
        superfilter.InitializeFieldSelectors<User>();

        List<User> result = superfilter.ApplyConfiguredFilters(users).ToList();

        Assert.Equal(users.Count(), result.Count);
    }
}