using Database.Models;
using Superfilter;
using Superfilter.Constants;
using Superfilter.Entities;
using Tests.Common;

namespace Tests.Unit;

public class ExtensionApproachTests
{
    private static IQueryable<User> GetTestUsers()
    {
        return new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 100 },
            new() { Id = 2, Name = "Bob", MoneyAmount = 200 },
            new() { Id = 3, Name = "Charlie", MoneyAmount = 300 }
        }.AsQueryable();
    }

    [Fact]
    public void ExtensionApproach_WithManualMapping_ShouldWorkCorrectly()
    {
        IQueryable<User> users = GetTestUsers();
        HasFiltersDto filters = new(1, 10)
        {
            Filters = [new FilterCriterion("name", Operator.Equals, "Alice")]
        };

        List<User> result = users
            .WithSuperfilter()
            .MapProperty("name", x => x.Name)
            .WithFilters(filters)
            .ToList();

        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    [Fact]
    public void ExtensionApproach_WithFiltering_ShouldWorkCorrectly()
    {
        IQueryable<User> users = GetTestUsers();
        HasFiltersDto filters = new(1, 10)
        {
            Filters = [new FilterCriterion("name", Operator.Equals, "Bob")]
        };

        List<User> result = users
            .WithSuperfilter()
            .MapProperty("name", x => x.Name)
            .WithFilters(filters)
            .ToList();

        Assert.Single(result);
        Assert.Equal("Bob", result[0].Name);
    }

    [Fact]
    public void ExtensionApproach_WithNumericFilter_ShouldWorkCorrectly()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new(1, 10)
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.GreaterThan, "150")]
        };

        List<User> result = users
            .WithSuperfilter()
            .MapProperty("moneyAmount", x => x.MoneyAmount)
            .WithFilters(filters)
            .ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Name == "Bob");
        Assert.Contains(result, u => u.Name == "Charlie");
    }

    [Fact]
    public void ExtensionApproach_WithAutoPropertyMapping_ShouldWork()
    {
        IQueryable<User> users = GetTestUsers();
        HasFiltersDto filters = new(1, 10)
        {
            Filters = [new FilterCriterion("User.Name", Operator.Contains, "li")]
        };

        List<User> result = users
            .WithSuperfilter()
            .MapProperty(x => x.Name) // Auto-maps to "User.Name"
            .WithFilters(filters)
            .ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Name == "Alice");
        Assert.Contains(result, u => u.Name == "Charlie");
    }
}