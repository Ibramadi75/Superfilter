using System.Linq;
using System.Linq.Expressions;
using Xunit;
using SuperFilter;
using Database.Models;

public class HasFiltersDto : IHasFilters
{
    public List<FilterCriterion> Filters { get; set; }
}

public class SuperFilterTests
{
    private GlobalConfiguration GetGlobalConfiguration()
    {
        return new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("id", Operator.GreaterThan, "1"),
                    new FilterCriterion("name", Operator.Contains, "e"),
                    new FilterCriterion("moneyAmount", Operator.GreaterThan, "100"),
                    new FilterCriterion("idCar", Operator.LessThan, "200"),
                    new FilterCriterion("nameCar", Operator.StartsWith, "f")
                ]
            }
        };
    }

    private IQueryable<User> GetTestUsers()
    {
        return new List<User>
        {
            new User { Id = 1, Name = "Alice", MoneyAmount = 150, Car = new Car { Id = 1, Name = "Ford" }},
            new User { Id = 2, Name = "Bob", MoneyAmount = 200, Car = new Car { Id = 2, Name = "Fiat" }},
            new User { Id = 3, Name = "Charlie", MoneyAmount = 50, Car = new Car { Id = 3, Name = "BMW" }},
            new User { Id = 4, Name = "Dave", MoneyAmount = 300, Car = new Car { Id = 4, Name = "Honda" }},
        }.AsQueryable();
    }

    [Fact]
    public void FilterProperty_ValidFilter_ApplyFilterCorrectly()
    {
        var users = GetTestUsers();
        var globalConfiguration = GetGlobalConfiguration();

        var superFilter = new SuperFilter.SuperFilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "id", new FieldConfiguration { DtoPropertyName = "id", Selector = (Expression<Func<User, object>>)(x => x.Id), IsRequired = true } },
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Test filtering by "id"
        var result = superFilter.ApplyFilters(users).ToList();

        Assert.Equal(3, result.Count); // Only users with Id > 1 should remain
        Assert.DoesNotContain(result, user => user.Id == 1); // User with Id = 1 should be excluded
    }

    [Fact]
    public void FilterProperty_NoMatchingFilter_EmptyResult()
    {
        var users = GetTestUsers();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("name", Operator.Contains, "nonexistent"),
                ]
            }
        };

        var superFilter = new SuperFilter.SuperFilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "name", new FieldConfiguration { DtoPropertyName = "name", Selector = (Expression<Func<User, object>>)(x => x.Name), IsRequired = true } },
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Test filtering with no matching filter (no users should match)
        var result = superFilter.ApplyFilters(users).ToList();

        Assert.Empty(result); // No users should match "nonexistent"
    }

    [Fact]
    public void FilterProperty_FieldNotInCriteria_NoFilterApplied()
    {
        var users = GetTestUsers();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("unknownField", Operator.Contains, "e")
                ]
            }
        };

        var superFilter = new SuperFilter.SuperFilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "name", new FieldConfiguration { DtoPropertyName = "name", Selector = (Expression<Func<User, object>>)(x => x.Name), IsRequired = false } },
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Test filtering by a field that's not in the DTO (filter should be ignored)
        var result = superFilter.ApplyFilters(users).ToList();

        Assert.Equal(4, result.Count); // All users should remain, as the filter is ignored
    }

    [Fact]
    public void FilterProperty_EmptyValueInFilter_SkipFiltering()
    {
        var users = GetTestUsers();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("name", Operator.Contains, "")
                ]
            }
        };

        var superFilter = new SuperFilter.SuperFilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "id", new FieldConfiguration { DtoPropertyName = "id", Selector = (Expression<Func<User, object>>)(x => x.Name), IsRequired = false } },
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Test filtering with an empty value (no filter should be applied)
        var result = superFilter.ApplyFilters(users).ToList();

        Assert.Equal(4, result.Count); // No filtering should be applied, all users should remain
    }

    [Fact]
    public void FilterProperty_SuperFilterExceptionThrown_WhenRequiredFilterIsMissing()
    {
        var users = GetTestUsers();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("name", Operator.Contains, "e")
                ]
            }
        };

        var superFilter = new SuperFilter.SuperFilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "id", new FieldConfiguration { DtoPropertyName = "id", Selector = (Expression<Func<User, object>>)(x => x.Id), IsRequired = true } },
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Test that an exception is thrown when a required filter is missing
        var exception = Assert.Throws<SuperFilterException>(() => superFilter.ApplyFilters(users));

        Assert.Equal("Filter id is required.", exception.Message);
    }
}