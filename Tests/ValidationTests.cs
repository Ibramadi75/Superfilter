using System.Linq.Expressions;
using Database.Models;
using SuperFilter;
using SuperFilter.Entities;

namespace Tests;

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
    public void FilterProperty_SuperFilterExceptionThrown_WhenRequiredFilterIsMissing()
    {
        IQueryable<User> users = GetTestUsers();
        GlobalConfiguration globalConfiguration = new()
        {
            HasFilters = new HasFiltersDto
            {
                Filters = [new FilterCriterion("name", Operator.Contains, "e")]
            }
        };

        Superfilter superfilter = new();
        Dictionary<string, FieldConfiguration> propertyMappings = new()
        {
            { "id", new FieldConfiguration { EntityPropertyName = "id", Selector = (Expression<Func<User, object>>)(x => x.Id), IsRequired = true } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superfilter.InitializeGlobalConfiguration(globalConfiguration);
        superfilter.InitializeFieldSelectors<User>();

        SuperFilterException exception = Assert.Throws<SuperFilterException>(() => superfilter.ApplyConfiguredFilters(users));

        Assert.Equal("Filter id is required.", exception.Message);
    }

    [Fact]
    public void FilterProperty_WithInvalidOperatorForStringType_ThrowsException()
    {
        IQueryable<User> users = GetTestUsers();
        GlobalConfiguration globalConfiguration = new()
        {
            HasFilters = new HasFiltersDto
            {
                Filters = [new FilterCriterion("name", Operator.GreaterThan, "test")]
            }
        };

        Superfilter superfilter = new();
        Dictionary<string, FieldConfiguration> propertyMappings = new()
        {
            { "name", new FieldConfiguration { EntityPropertyName = "name", Selector = (Expression<Func<User, object>>)(x => x.Name), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superfilter.InitializeGlobalConfiguration(globalConfiguration);
        superfilter.InitializeFieldSelectors<User>();

        Assert.Throws<SuperFilterException>(() => superfilter.ApplyConfiguredFilters(users));
    }

    [Fact]
    public void FilterProperty_WithNullValue_SkipsFilter()
    {
        IQueryable<User> users = GetTestUsers();
        GlobalConfiguration globalConfiguration = new()
        {
            HasFilters = new HasFiltersDto
            {
                Filters = [new FilterCriterion("name", Operator.Contains, null!)]
            }
        };

        Superfilter superfilter = new();
        Dictionary<string, FieldConfiguration> propertyMappings = new()
        {
            { "name", new FieldConfiguration { EntityPropertyName = "name", Selector = (Expression<Func<User, object>>)(x => x.Name), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superfilter.InitializeGlobalConfiguration(globalConfiguration);
        superfilter.InitializeFieldSelectors<User>();

        List<User> result = superfilter.ApplyConfiguredFilters(users).ToList();

        Assert.Equal(users.Count(), result.Count);
    }

    [Fact]
    public void FilterProperty_WithWhitespaceValue_FiltersWithWhitespace()
    {
        IQueryable<User> users = GetTestUsers();
        GlobalConfiguration globalConfiguration = new()
        {
            HasFilters = new HasFiltersDto
            {
                Filters = [new FilterCriterion("name", Operator.Contains, "   ")]
            }
        };

        Superfilter superfilter = new();
        Dictionary<string, FieldConfiguration> propertyMappings = new()
        {
            { "name", new FieldConfiguration { EntityPropertyName = "name", Selector = (Expression<Func<User, object>>)(x => x.Name), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superfilter.InitializeGlobalConfiguration(globalConfiguration);
        superfilter.InitializeFieldSelectors<User>();

        List<User> result = superfilter.ApplyConfiguredFilters(users).ToList();

        Assert.Empty(result);
    }
}