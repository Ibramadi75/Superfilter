using System.Globalization;
using System.Linq.Expressions;
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
                    new FilterCriterion("nameCar", Operator.StartsWith, "f"),
                    new FilterCriterion("houseAddress", Operator.Contains, "Street"),
                    new FilterCriterion("bornDate", Operator.IsEqualToYear, "23/02/2003")
                ]
            }
        };
    }

    private IQueryable<User> GetTestUsers()
    {
        var brandFord = new Brand { Id = 1, Name = "Ford", Rate = 5 };
        var brandFiat = new Brand { Id = 2, Name = "Fiat", Rate = 3 };
        var brandBMW = new Brand { Id = 3, Name = "BMW", Rate = 4 };
        var brandHonda = new Brand { Id = 4, Name = "Honda", Rate = 2 };
        
        var mayor = new User { Id = 999, Name = "SuperMayor", MoneyAmount = 150, 
            Car = new Car { Id = 1, Name = "Ford Fiesta", Brand = brandFiat }, 
            House = new House { Id = 1, Address = "123 Main Street" } };
        var paris = new City() { Id = 1, Name = "Paris", Mayor = mayor, MayorId = mayor.Id };
        var chatillon = new City() { Id = 2, Name = "Chatillon" };
        
        return new List<User>
        {
            new User { Id = 1, Name = "Alice", MoneyAmount = 150, 
                Car = new Car { Id = 1, Name = "Ford Fiesta", Brand = brandFord }, 
                House = new House { Id = 1, Address = "123 Main Street", City = paris, CityId = 2}, 
            },
            new User { Id = 2, Name = "Bob", MoneyAmount = 200, BornDate = DateTime.ParseExact("10/12/2003", "dd/MM/yyyy", CultureInfo.InvariantCulture),
                Car = new Car { Id = 2, Name = "Fiat 500", Brand = brandFiat }, 
                House = new House { Id = 2, Address = "456 Oak Street", City = chatillon, CityId = 2} 
            },
            new User { Id = 3, Name = "Charlie", MoneyAmount = 50, 
                Car = new Car { Id = 3, Name = "BMW X3", Brand = brandBMW }, 
                House = new House { Id = 3, Address = "789 Pine Avenue" } 
            },
            new User { Id = 4, Name = "Dave", MoneyAmount = 300, 
                Car = new Car { Id = 4, Name = "Honda Civic", Brand = brandHonda }, 
                House = new House { Id = 4, Address = "101 Maple Road" } 
            },
            mayor
        }.AsQueryable();
    }

    [Fact]
    public void FilterProperty_OnInteger_ValidFilter_ApplyFilterCorrectly()
    {
        var users = GetTestUsers();
        
        
        var globalConfiguration = GetGlobalConfiguration();

        var superFilter = new SuperFilter.SuperFilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "id", new FieldConfiguration { EntityPropertyName = nameof(User.Id), Selector = (Expression<Func<User, object>>)(x => x.Id), IsRequired = true } },
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Test filtering by "id"
        var result = superFilter.ApplyFilters(users).ToList();

        Assert.Equal(users.Count()-1, result.Count); // Only users with Id > 1 should remain
        Assert.DoesNotContain(result, user => user.Id == 1); // User with Id = 1 should be excluded
    }
    
    [Fact]
    public void FilterProperty_OnDate_ValidFilter_ApplyFilterCorrectly()
    {
        var users = GetTestUsers();
        var globalConfiguration = GetGlobalConfiguration();

        var superFilter = new SuperFilter.SuperFilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "bornDate", new FieldConfiguration { EntityPropertyName = "bornDate", Selector = (Expression<Func<User, object>>)(x => x.BornDate) } },
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Test filtering by "id"
        var result = superFilter.ApplyFilters(users).ToList();

        Assert.Equal(1, result.Count); // Only one user should remain
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
            { "name", new FieldConfiguration { EntityPropertyName = "name", Selector = (Expression<Func<User, object>>)(x => x.Name), IsRequired = true } },
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
            { "name", new FieldConfiguration { EntityPropertyName = "name", Selector = (Expression<Func<User, object>>)(x => x.Name), IsRequired = false } },
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Test filtering by a field that's not in the DTO (filter should be ignored)
        var result = superFilter.ApplyFilters(users).ToList();

        Assert.Equal(users.Count(), result.Count); // All users should remain, as the filter is ignored
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
            { "id", new FieldConfiguration { EntityPropertyName = "id", Selector = (Expression<Func<User, object>>)(x => x.Name), IsRequired = false } },
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Test filtering with an empty value (no filter should be applied)
        var result = superFilter.ApplyFilters(users).ToList();

        Assert.Equal(users.Count(), result.Count); // No filtering should be applied, all users should remain
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
            { "id", new FieldConfiguration { EntityPropertyName = "id", Selector = (Expression<Func<User, object>>)(x => x.Id), IsRequired = true } },
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Test that an exception is thrown when a required filter is missing
        var exception = Assert.Throws<SuperFilterException>(() => superFilter.ApplyFilters(users));

        Assert.Equal("Filter id is required.", exception.Message);
    }

    [Fact]
    public void FilterByCarBrandName_FiltersCorrectly()
    {
        var users = GetTestUsers();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("carBrandName", Operator.Equals, "BMW")
                ]
            }
        };

        var superFilter = new SuperFilter.SuperFilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "carBrandName", new FieldConfiguration { EntityPropertyName = "name", Selector = (Expression<Func<User, object>>)(x => x.Car.Brand.Name), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        var result = superFilter.ApplyFilters(users).ToList();

        Assert.Single(result);
        Assert.Contains(result, user => user.Car.Brand.Name == "BMW");
    }

    [Fact]
    public void FilterByHouseAddress_FiltersCorrectly()
    {
        var users = GetTestUsers();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("houseAddress", Operator.Contains, "Oak")
                ]
            }
        };

        var superFilter = new SuperFilter.SuperFilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "houseAddress", new FieldConfiguration { EntityPropertyName = "address", Selector = (Expression<Func<User, object>>)(x => x.House.Address), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        var result = superFilter.ApplyFilters(users).ToList();

        Assert.Single(result);
        Assert.Contains(result, user => user.House.Address.Contains("Oak"));
    }

    [Fact]
    public void FilterByCarBrandRate_FiltersCorrectly()
    {
        var users = GetTestUsers();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("carBrandRate", Operator.GreaterThan, "3")
                ]
            }
        };

        var superFilter = new SuperFilter.SuperFilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "carBrandRate", new FieldConfiguration { EntityPropertyName = "rate", Selector = (Expression<Func<User, object>>)(x => x.Car.Brand.Rate), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        var result = superFilter.ApplyFilters(users).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, user => user.Car.Brand.Rate == 5);
        Assert.Contains(result, user => user.Car.Brand.Rate == 4);
    }
    
    [Fact]
    public void FilterByDoubleNestedProperty_FiltersCorrectly()
    {
        // Arrange
        var users = GetTestUsers();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("carBrandName", Operator.Equals, "Ford") // Filtre sur Car.Brand.Name
                ]
            }
        };

        var superFilter = new SuperFilter.SuperFilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "carBrandName", new FieldConfiguration { EntityPropertyName = nameof(Brand.Name), Selector = (Expression<Func<User, object>>)(x => x.Car.Brand.Name), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Act
        var result = superFilter.ApplyFilters(users).ToList();

        // Assert
        Assert.Single(result); // Un seul utilisateur devrait correspondre
        Assert.Contains(result, user => user.Car.Brand.Name == "Ford"); // Vérifie que le filtre a fonctionné
    }
}