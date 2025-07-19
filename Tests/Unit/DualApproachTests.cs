using Database.Models;
using Superfilter;
using Superfilter.Constants;
using Superfilter.Entities;
using Tests.Common;

namespace Tests.Unit;

public class DualApproachTests
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
    public void BuilderApproach_ShouldWorkCorrectly()
    {
        IQueryable<User> users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("name", Operator.Equals, "Alice")]
        };

        List<User> result = SuperfilterBuilder.For<User>()
            .MapProperty("name", x => x.Name)
            .WithFilters(filters)
            .Build(users).ToList();

        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    [Fact]
    public void ExtensionApproach_ShouldWorkCorrectly()
    {
        IQueryable<User> users = GetTestUsers();
        var filters = new HasFiltersDto
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
    public void BothApproaches_ShouldGiveSameResults()
    {
        IQueryable<User> users1 = GetTestUsers();
        IQueryable<User> users2 = GetTestUsers();
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.GreaterThan, "150")]
        };

        // Builder approach
        List<User> builderResult = SuperfilterBuilder.For<User>()
            .MapProperty("moneyAmount", x => x.MoneyAmount)
            .WithFilters(filters)
            .Build(users1).ToList();

        // Extension approach
        List<User> extensionResult = users2
            .WithSuperfilter()
            .MapProperty("moneyAmount", x => x.MoneyAmount)
            .WithFilters(filters)
            .ToList();

        // Both should return same results
        Assert.Equal(builderResult.Count, extensionResult.Count);
        Assert.Equal(2, builderResult.Count);
        Assert.Equal(2, extensionResult.Count);
        
        var builderIds = builderResult.Select(u => u.Id).OrderBy(x => x).ToList();
        var extensionIds = extensionResult.Select(u => u.Id).OrderBy(x => x).ToList();
        
        Assert.Equal(builderIds, extensionIds);
    }

    [Fact]
    public void ExtensionApproach_WithAutoPropertyMapping_ShouldWork()
    {
        IQueryable<User> users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("User.Name", Operator.Contains, "li")]
        };

        List<User> result = users
            .WithSuperfilter()
            .MapProperty(x => x.Name)  // Auto-maps to "User.Name"
            .WithFilters(filters)
            .ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Name == "Alice");
        Assert.Contains(result, u => u.Name == "Charlie");
    }
}