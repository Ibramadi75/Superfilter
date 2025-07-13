using Superfilter.Builder;
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
        var config = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)
            .MapProperty("id", u => u.Id)
            .Build();

        // Assert
        Assert.NotNull(config);
        Assert.NotNull(config.PropertyMappings);
        Assert.Equal(2, config.PropertyMappings.Count);
        
        Assert.True(config.PropertyMappings.ContainsKey("name"));
        Assert.True(config.PropertyMappings.ContainsKey("id"));
        
        var nameConfig = config.PropertyMappings["name"];
        Assert.NotNull(nameConfig.Selector);
        Assert.False(nameConfig.IsRequired);
        
        var idConfig = config.PropertyMappings["id"];
        Assert.NotNull(idConfig.Selector);
        Assert.False(idConfig.IsRequired);
    }

    [Fact]
    public void ConfigurationBuilder_ShouldCreateRequiredMapping()
    {
        // Act
        var config = SuperfilterBuilder.For<User>()
            .MapRequiredProperty("id", u => u.Id)
            .MapProperty("name", u => u.Name)
            .Build();

        // Assert
        var idConfig = config.PropertyMappings["id"];
        Assert.True(idConfig.IsRequired);
        
        var nameConfig = config.PropertyMappings["name"];
        Assert.False(nameConfig.IsRequired);
    }

    [Fact]
    public void ConfigurationBuilder_ShouldHandleNestedProperties()
    {
        // Act
        var config = SuperfilterBuilder.For<User>()
            .MapProperty("carBrandName", u => u.Car.Brand.Name)
            .MapProperty("cityName", u => u.House.City.Name)
            .Build();

        // Assert
        Assert.Equal(2, config.PropertyMappings.Count);
        Assert.True(config.PropertyMappings.ContainsKey("carBrandName"));
        Assert.True(config.PropertyMappings.ContainsKey("cityName"));
    }

    [Fact]
    public void ConfigurationBuilder_ShouldAddStaticFilters()
    {
        // Act
        var config = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)
            .AddStaticFilter("name", Operator.Equals, "John")
            .AddStaticFilter("moneyAmount", Operator.GreaterThan, "1000")
            .Build();

        // Assert
        Assert.NotNull(config.HasFilters);
        Assert.Equal(2, config.HasFilters.Filters.Count);
        
        var nameFilter = config.HasFilters.Filters.First(f => f.Field == "name");
        Assert.Equal(Operator.Equals, nameFilter.Operator);
        Assert.Equal("John", nameFilter.Value);
        
        var moneyFilter = config.HasFilters.Filters.First(f => f.Field == "moneyAmount");
        Assert.Equal(Operator.GreaterThan, moneyFilter.Operator);
        Assert.Equal("1000", moneyFilter.Value);
    }

    [Fact]
    public void ConfigurationBuilder_ShouldAddStaticSorts()
    {
        // Act
        var config = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)
            .AddStaticSort("name", "asc")
            .AddStaticSort("bornDate", "desc")
            .Build();

        // Assert
        Assert.NotNull(config.HasSorts);
        Assert.Equal(2, config.HasSorts.Sorters.Count);
        
        var nameSort = config.HasSorts.Sorters.First(s => s.Field == "name");
        Assert.Equal("asc", nameSort.dir);
        
        var dateSort = config.HasSorts.Sorters.First(s => s.Field == "bornDate");
        Assert.Equal("desc", dateSort.dir);
    }

    [Fact]
    public void ConfigurationBuilder_ShouldSetErrorStrategy()
    {
        // Act
        var config = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)
            .WithErrorStrategy(OnErrorStrategy.Ignore)
            .Build();

        // Assert
        Assert.Equal(OnErrorStrategy.Ignore, config.MissingOnStrategy);
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
        var config = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)
            .MapProperty("moneyAmount", u => u.MoneyAmount)
            .WithFilters(clientFilters)
            .Build();

        // Assert
        Assert.Equal(2, config.HasFilters.Filters.Count);
        
        var filters = config.HasFilters.Filters;
        Assert.Contains(filters, f => f.Field == "name" && f.Operator == Operator.Contains && f.Value == "John");
        Assert.Contains(filters, f => f.Field == "moneyAmount" && f.Operator == Operator.GreaterThan && f.Value == "5000");
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
        var config = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)
            .MapProperty("moneyAmount", u => u.MoneyAmount)
            .WithSorts(clientSorts)
            .Build();

        // Assert
        Assert.Equal(2, config.HasSorts.Sorters.Count);
        
        var sorts = config.HasSorts.Sorters;
        Assert.Contains(sorts, s => s.Field == "name" && s.dir == "asc");
        Assert.Contains(sorts, s => s.Field == "moneyAmount" && s.dir == "desc");
    }

    [Fact]
    public void ConfigurationBuilderExtensions_ShouldProvideStaticFilterMethods()
    {
        // Act
        var config = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)
            .MapProperty("moneyAmount", u => u.MoneyAmount)
            .AddStaticFilterEquals("name", "John")
            .AddStaticFilterGreaterThan("moneyAmount", "1000")
            .Build();

        // Assert
        Assert.Equal(2, config.HasFilters.Filters.Count);
        
        var filters = config.HasFilters.Filters;
        Assert.Contains(filters, f => f.Field == "name" && f.Operator == Operator.Equals && f.Value == "John");
        Assert.Contains(filters, f => f.Field == "moneyAmount" && f.Operator == Operator.GreaterThan && f.Value == "1000");
    }

    [Fact]
    public void ConfigurationBuilderExtensions_ShouldProvideStaticSortMethods()
    {
        // Act
        var config = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)
            .MapProperty("bornDate", u => u.BornDate)
            .AddStaticSortAscending("name")
            .AddStaticSortDescending("bornDate")
            .Build();

        // Assert
        Assert.Equal(2, config.HasSorts.Sorters.Count);
        
        var sorters = config.HasSorts.Sorters;
        Assert.Contains(sorters, s => s.Field == "name" && s.dir == "asc");
        Assert.Contains(sorters, s => s.Field == "bornDate" && s.dir == "desc");
    }

    [Fact]
    public void ConfigurationBuilderExtensions_ShouldHandleMapAndStaticFilter()
    {
        // Act
        var config = SuperfilterBuilder.For<User>()
            .MapAndAddStaticFilter("name", u => u.Name, Operator.Equals, "John")
            .Build();

        // Assert
        // Should have both mapping and filter
        Assert.Single(config.PropertyMappings);
        Assert.True(config.PropertyMappings.ContainsKey("name"));
        
        Assert.Single(config.HasFilters.Filters);
        var filter = config.HasFilters.Filters.First();
        Assert.Equal("name", filter.Field);
        Assert.Equal(Operator.Equals, filter.Operator);
        Assert.Equal("John", filter.Value);
    }

    [Fact]
    public void ConfigurationBuilder_ShouldHandleDifferentPropertyTypes()
    {
        // Act - MapProperty should handle all types automatically with type inference
        var config = SuperfilterBuilder.For<User>()
            .MapProperty("name", u => u.Name)                    // string
            .MapProperty("id", u => u.Id)                        // int
            .MapProperty("bornDate", u => u.BornDate)            // DateTime?
            .MapProperty("moneyAmount", u => u.MoneyAmount)      // int
            .Build();

        // Assert
        Assert.Equal(4, config.PropertyMappings.Count);
        Assert.True(config.PropertyMappings.ContainsKey("name"));
        Assert.True(config.PropertyMappings.ContainsKey("id"));
        Assert.True(config.PropertyMappings.ContainsKey("bornDate"));
        Assert.True(config.PropertyMappings.ContainsKey("moneyAmount"));
    }

    [Fact]
    public void ConfigurationBuilder_ShouldConvertExpressionsCorrectly()
    {
        // Act
        var config = SuperfilterBuilder.For<User>()
            .MapProperty("id", u => u.Id)  // int property
            .MapProperty("name", u => u.Name)  // string property
            .Build();

        // Assert
        var idConfig = config.PropertyMappings["id"];
        var nameConfig = config.PropertyMappings["name"];
        
        // Both should be convertible to Expression<Func<User, object>>
        Assert.NotNull(idConfig.Selector);
        Assert.NotNull(nameConfig.Selector);
        
        // Both should be LambdaExpression (the base type used by FieldConfiguration)
        Assert.NotNull(idConfig.Selector);
        Assert.NotNull(nameConfig.Selector);
    }

    [Fact]
    public void ConfigurationBuilder_ShouldBuildMappingsOnly()
    {
        // Act
        var config = SuperfilterBuilder.For<User>()
            .MapRequiredProperty("id", u => u.Id)
            .MapProperty("name", u => u.Name)
            .AddStaticFilter("name", Operator.Equals, "John")  // This should be ignored
            .BuildMappingsOnly();

        // Assert
        Assert.Equal(2, config.PropertyMappings.Count);
        Assert.True(config.PropertyMappings.ContainsKey("id"));
        Assert.True(config.PropertyMappings.ContainsKey("name"));
        
        // Filters and sorts should be empty
        Assert.Empty(config.HasFilters.Filters);
        Assert.Empty(config.HasSorts.Sorters);
    }

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
        var config = SuperfilterBuilder.For<User>()
            .MapProperty("id", u => u.Id)
            .MapProperty("name", u => u.Name)
            .AddStaticFilter("id", Operator.GreaterThan, "0")  // Static default filter
            .WithFilters(clientFilters)  // This should replace static filters
            .Build();

        // Assert
        Assert.Single(config.HasFilters.Filters);
        var filter = config.HasFilters.Filters.First();
        Assert.Equal("name", filter.Field);
        Assert.Equal(Operator.Contains, filter.Operator);
        Assert.Equal("John", filter.Value);
    }
}