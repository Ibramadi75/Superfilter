using Database.Models;
using Superfilter;
using Superfilter.Constants;
using Superfilter.Entities;
using Tests.Common;

namespace Tests.Unit;

public class ValidationTests
{
    private static IQueryable<User> GetTestUsers()
    {
        Brand brandFord = new() { Id = 1, Name = "Ford", Rate = 5 };

        return new List<User>
        {
            new()
            {
                Id = 1, Name = "Alice", MoneyAmount = 150,
                Car = new Car { Id = 1, Name = "Ford Fiesta", Brand = brandFord }
            },
            new()
            {
                Id = 2, Name = "Bob", MoneyAmount = 200,
                Car = new Car { Id = 2, Name = "Fiat 500" }
            }
        }.AsQueryable();
    }

    [Fact]
    public void FilterProperty_SuperfilterExceptionThrown_WhenRequiredFilterIsMissing()
    {
        IQueryable<User> users = GetTestUsers();
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("name", Operator.Contains, "e")]
        };

        SuperfilterException exception = Assert.Throws<SuperfilterException>(() => 
            SuperfilterBuilder.For<User>()
                .MapRequiredProperty("id", x => x.Id)
                .WithFilters(filters)
                .Build(users).ToList());

        Assert.Equal("Filter id is required.", exception.Message);
    }

    [Fact]
    public void FilterProperty_WithInvalidOperatorForStringType_ThrowsException()
    {
        IQueryable<User> users = GetTestUsers();
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("name", Operator.GreaterThan, "test")]
        };

        Assert.Throws<SuperfilterException>(() => 
            SuperfilterBuilder.For<User>()
                .MapProperty("name", x => x.Name)
                .WithFilters(filters)
                .Build(users).ToList());
    }

    [Fact]
    public void FilterProperty_WithNullValue_SkipsFilter()
    {
        IQueryable<User> users = GetTestUsers();
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("name", Operator.Contains, null!)]
        };

        List<User> result = SuperfilterBuilder.For<User>()
            .MapProperty("name", x => x.Name)
            .WithFilters(filters)
            .Build(users).ToList();

        Assert.Equal(GetTestUsers().Count(), result.Count);
    }

    [Fact]
    public void FilterProperty_WithWhitespaceValue_FiltersWithWhitespace()
    {
        IQueryable<User> users = GetTestUsers();
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("name", Operator.Contains, "   ")]
        };

        List<User> result = SuperfilterBuilder.For<User>()
            .MapProperty("name", x => x.Name)
            .WithFilters(filters)
            .Build(users).ToList();

        Assert.Empty(result);
    }
}