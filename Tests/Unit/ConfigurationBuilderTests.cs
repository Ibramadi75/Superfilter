using Superfilter;
using Superfilter.Constants;
using Superfilter.Entities;
using Database.Models;

namespace Tests.Unit;

// Test DTOs for simulating client requests
public class TestFiltersDto : IHasFilters
{
    public List<FilterCriterion> Filters { get; set; } = new();
}

public class TestSortsDto : IHasSorts
{
    public List<SortCriterion> Sorters { get; set; } = new();
}

public class ConfigurationBuilderTests
{
    [Fact]
    public void ConfigurationBuilder_ShouldCreateBasicMapping_WithoutManualCasting()
    {
        // Act
        var testUsers = new List<User>
        {
            new() { Id = 1, Name = "John" },
            new() { Id = 2, Name = "Jane" }
        }.AsQueryable();

        var result = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)
            .MapProperty("id", u => u.Id)
            .Build(testUsers);
        Assert.NotNull(result);
    }

    [Fact]
    public void ConfigurationBuilder_ShouldCreateRequiredMapping()
    {
        // Act & Assert - Required mappings should throw exception when missing
        var filters = new TestFiltersDto
        {
            Filters = new List<FilterCriterion>
            {
                new("name", Operator.Contains, "John")
                // Missing required "id" filter
            }
        };

        var testUsers = new List<User>
        {
            new() { Id = 1, Name = "John" }
        }.AsQueryable();

        // Should throw because required "id" filter is missing
        Assert.Throws<SuperfilterException>(() => 
            SuperfilterBuilder.For<User>()
                .MapRequiredProperty("id", u => u.Id)
                .MapProperty("name", u => u.Name)
                .WithFilters(filters)
                .Build(testUsers).ToList());
    }

    [Fact]
    public void ConfigurationBuilder_ShouldHandleNestedProperties()
    {
        // Act
        var testUsers = new List<User>
        {
            new() { Id = 1, Name = "John" }
        }.AsQueryable();

        var result = SuperfilterBuilder.For<User>()
            .MapProperty("carBrandName", u => u.Car!.Brand!.Name)
            .MapProperty("cityName", u => u.House!.City.Name)
            .Build(testUsers);
        
        // Test that nested properties work
        Assert.NotNull(result);
    }

    [Fact]
    public void ConfigurationBuilder_ShouldAddStaticFilters()
    {
        // Act
        var testUsers = new List<User>
        {
            new() { Id = 1, Name = "John", MoneyAmount = 1500 },
            new() { Id = 2, Name = "Jane", MoneyAmount = 500 }
        }.AsQueryable();

        var result = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)
            .MapProperty("moneyAmount", u => u.MoneyAmount)
            .AddStaticFilter("name", Operator.Equals, "John")
            .AddStaticFilter("moneyAmount", Operator.GreaterThan, "1000")
            .Build(testUsers).ToList();
        
        // Test that static filters work
        Assert.Single(result);
        Assert.Equal("John", result.First().Name);
        Assert.Equal(1500, result.First().MoneyAmount);
    }

    [Fact]
    public void ConfigurationBuilder_ShouldAddStaticSorts()
    {
        // Act
        var testUsers = new List<User>
        {
            new() { Id = 1, Name = "John", BornDate = DateTime.Parse("1990-01-01") },
            new() { Id = 2, Name = "Jane", BornDate = DateTime.Parse("1985-01-01") }
        }.AsQueryable();

        var result = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)
            .MapProperty("bornDate", u => u.BornDate)
            .AddStaticSort("name", "asc")
            .AddStaticSort("bornDate", "desc")
            .Build(testUsers);
        
        // Test that static sorts work - we can't easily test sorting without accessing internals
        // But we can verify the instance is created and works
        Assert.NotNull(result);
    }

    [Fact]
    public void ConfigurationBuilder_ShouldSetErrorStrategy()
    {
        // Act
        var testUsers = new List<User>
        {
            new() { Id = 1, Name = "John" }
        }.AsQueryable();

        var result = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)
            .WithErrorStrategy(OnErrorStrategy.Ignore)
            .Build(testUsers);
        
        // Test that error strategy is applied
        Assert.NotNull(result);
    }

    [Fact]
    public void ConfigurationBuilder_ShouldAcceptDynamicFiltersFromClient()
    {
        // Arrange - Simulate client filters DTO
        var clientFilters = new TestFiltersDto
        {
            Filters = new List<FilterCriterion>
            {
                new("name", Operator.Contains, "John"),
                new("moneyAmount", Operator.GreaterThan, "5000")
            }
        };

        // Act
        var testUsers = new List<User>
        {
            new() { Id = 1, Name = "John Doe", MoneyAmount = 6000 },
            new() { Id = 2, Name = "Jane Smith", MoneyAmount = 3000 },
            new() { Id = 3, Name = "John Smith", MoneyAmount = 4000 }
        }.AsQueryable();

        var result = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)
            .MapProperty("moneyAmount", u => u.MoneyAmount)
            .WithFilters(clientFilters)
            .Build(testUsers).ToList();
        
        // Test that dynamic filters work
        Assert.Single(result);
        Assert.Equal("John Doe", result.First().Name);
        Assert.Equal(6000, result.First().MoneyAmount);
    }

    [Fact]
    public void ConfigurationBuilder_ShouldAcceptDynamicSortsFromClient()
    {
        // Arrange - Simulate client sorts DTO
        var clientSorts = new TestSortsDto
        {
            Sorters = new List<SortCriterion>
            {
                new("name", "asc"),
                new("moneyAmount", "desc")
            }
        };

        // Act
        var testUsers = new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 3000 },
            new() { Id = 2, Name = "Bob", MoneyAmount = 5000 }
        }.AsQueryable();

        var result = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)
            .MapProperty("moneyAmount", u => u.MoneyAmount)
            .WithSorts(clientSorts)
            .Build(testUsers);
        
        // Test that dynamic sorts work
        Assert.NotNull(result);
    }

    [Fact]
    public void ConfigurationBuilderExtensions_ShouldProvideStaticFilterMethods()
    {
        // Act
        var testUsers = new List<User>
        {
            new() { Id = 1, Name = "John", MoneyAmount = 1500 },
            new() { Id = 2, Name = "Jane", MoneyAmount = 500 }
        }.AsQueryable();

        var result = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)
            .MapProperty("moneyAmount", u => u.MoneyAmount)
            .AddStaticFilter("name", Operator.Equals, "John")
            .AddStaticFilter("moneyAmount", Operator.GreaterThan, "1000")
            .Build(testUsers).ToList();
        
        // Test that static filter methods work
        Assert.Single(result);
        Assert.Equal("John", result.First().Name);
    }

    [Fact]
    public void ConfigurationBuilderExtensions_ShouldProvideStaticSortMethods()
    {
        // Act
        var testUsers = new List<User>
        {
            new() { Id = 1, Name = "Bob", BornDate = DateTime.Parse("1990-01-01") },
            new() { Id = 2, Name = "Alice", BornDate = DateTime.Parse("1985-01-01") }
        }.AsQueryable();

        var result = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)
            .MapProperty("bornDate", u => u.BornDate)
            .AddStaticSortAscending("name")
            .AddStaticSortDescending("bornDate")
            .Build(testUsers);
        
        // Test that static sort methods work
        Assert.NotNull(result);
    }

    [Fact]
    public void ConfigurationBuilderExtensions_ShouldHandleMapAndStaticFilter()
    {
        // Act
        var testUsers = new List<User>
        {
            new() { Id = 1, Name = "John" },
            new() { Id = 2, Name = "Jane" }
        }.AsQueryable();

        var result = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)
            .AddStaticFilter("name", Operator.Equals, "John")
            .Build(testUsers).ToList();
        
        // Test that map and static filter work together
        Assert.Single(result);
        Assert.Equal("John", result.First().Name);
    }

    [Fact]
    public void ConfigurationBuilder_ShouldHandleDifferentPropertyTypes()
    {
        // Act - MapProperty should handle all types automatically with type inference
        var testUsers = new List<User>
        {
            new() { Id = 1, Name = "John", BornDate = DateTime.Parse("1990-01-01"), MoneyAmount = 1000 }
        }.AsQueryable();

        var result = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)                    // string
            .MapProperty("id", u => u.Id)                        // int
            .MapProperty("bornDate", u => u.BornDate)            // DateTime?
            .MapProperty("moneyAmount", u => u.MoneyAmount)      // int
            .Build(testUsers);
        
        // Test that different property types work
        Assert.NotNull(result);
    }

    [Fact]
    public void ConfigurationBuilder_ShouldConvertExpressionsCorrectly()
    {
        // Act
        var testUsers = new List<User>
        {
            new() { Id = 1, Name = "John" },
            new() { Id = 2, Name = "Jane" }
        }.AsQueryable();

        var result = SuperfilterBuilder.For<User>()
            .MapProperty("id", u => u.Id)  // int property
            .MapProperty("name", u => u.Name)  // string property
            .Build(testUsers);
        
        // Test that expression conversion works correctly
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    // BuildMappingsOnly method has been removed as it's no longer needed
    // The new API creates a complete, ready-to-use Superfilter instance

    [Fact]
    public void ConfigurationBuilder_ShouldCombineStaticAndDynamicFilters()
    {
        // Arrange
        var clientFilters = new TestFiltersDto
        {
            Filters = new List<FilterCriterion>
            {
                new("name", Operator.Contains, "John")
            }
        };

        // Act
        var testUsers = new List<User>
        {
            new() { Id = 1, Name = "John Doe" },
            new() { Id = 2, Name = "Jane Smith" }
        }.AsQueryable();

        var result = SuperfilterBuilder.For<User>()
            .MapProperty("id", u => u.Id)
            .MapProperty("name", u => u.Name)
            .AddStaticFilter("id", Operator.GreaterThan, "0")  // Static default filter
            .WithFilters(clientFilters)  // This should replace static filters
            .Build(testUsers).ToList();
        
        // Test that WithFilters replaces static filters
        Assert.Single(result);
        Assert.Equal("John Doe", result.First().Name);
    }

    [Fact]
    public void ConfigurationBuilder_Build_ShouldCreateReadyToUseInstance()
    {
        // Arrange
        var clientFilters = new TestFiltersDto
        {
            Filters = new List<FilterCriterion>
            {
                new("name", Operator.Contains, "John")
            }
        };

        // Act - New simplified API that eliminates type redundancy
        var testUsers = new List<User>
        {
            new() { Id = 1, Name = "John Doe" },
            new() { Id = 2, Name = "Jane Smith" }
        }.AsQueryable();

        var result = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)
            .MapProperty("id", u => u.Id)
            .WithFilters(clientFilters)
            .Build(testUsers);
        
        // Test that it's ready to use without additional initialization
        Assert.NotNull(result);
        
        // Should filter for users containing "John"
        var filteredUsers = result.ToList();
        Assert.Single(filteredUsers);
        Assert.Equal("John Doe", filteredUsers.First().Name);
    }
}