using System.Linq.Expressions;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Superfilter;
using Superfilter.Constants;
using Superfilter.Entities;
using Tests.Common;
using Xunit.Abstractions;

namespace Tests.Integration;

public class PostgreSqlIntegrationTests(ITestOutputHelper testOutputHelper) : PostgreSqlIntegrationTestBase
{
    [Fact]
    public async Task ApplyFilters_WithPostgreSQL_ShouldGenerateValidQuery()
    {
        var users = Context.Users.AsQueryable();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters = [new FilterCriterion("MoneyAmount", Operator.GreaterThan, "100")]
            }
        };

        var superfilter = new Superfilter.Superfilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "MoneyAmount", new FieldConfiguration 
                { 
                    EntityPropertyName = "MoneyAmount", 
                    Selector = (Expression<Func<User, object>>)(x => x.MoneyAmount), 
                    IsRequired = false 
                } 
            }
        };
        
        globalConfiguration.PropertyMappings = propertyMappings;
        superfilter.InitializeGlobalConfiguration(globalConfiguration);
        superfilter.InitializeFieldSelectors<User>();

        var filteredQuery = superfilter.ApplyConfiguredFilters(users);
        var sqlQuery = filteredQuery.ToQueryString();
        var result = await filteredQuery.ToListAsync();

        testOutputHelper.WriteLine("Generated PostgreSQL Query:");
        testOutputHelper.WriteLine(sqlQuery);
        testOutputHelper.WriteLine($"Connection String: {GetConnectionString()}");

        Assert.NotNull(sqlQuery);
        Assert.Contains("SELECT", sqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE", sqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"MoneyAmount\"", sqlQuery); // PostgreSQL uses double quotes for identifiers
        Assert.NotEmpty(result);
        Assert.All(result, user => Assert.True(user.MoneyAmount > 100));
    }

    [Fact]
    public async Task ApplyFilters_WithComplexNavigationProperties_ShouldWorkWithPostgreSQL()
    {
        var users = Context.Users
            .Include(u => u.Car)
            .ThenInclude(c => c!.Brand)
            .Include(u => u.House)
            .ThenInclude(h => h!.City)
            .AsQueryable();

        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters = 
                [
                    new FilterCriterion("carBrand", Operator.Equals, "Ford"),
                    new FilterCriterion("cityName", Operator.StartsWith, "P")
                ]
            }
        };

        var superfilter = new Superfilter.Superfilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "carBrand", new FieldConfiguration 
                { 
                    EntityPropertyName = "Name", 
                    Selector = (Expression<Func<User, object>>)(x => x.Car!.Brand!.Name), 
                    IsRequired = false 
                } 
            },
            { "cityName", new FieldConfiguration 
                { 
                    EntityPropertyName = "Name", 
                    Selector = (Expression<Func<User, object>>)(x => x.House!.City!.Name), 
                    IsRequired = false 
                } 
            }
        };
        
        globalConfiguration.PropertyMappings = propertyMappings;
        superfilter.InitializeGlobalConfiguration(globalConfiguration);
        superfilter.InitializeFieldSelectors<User>();
        
        var usersResult = await users.ToListAsync();

        var filteredQuery = superfilter.ApplyConfiguredFilters(users);
        var sqlQuery = filteredQuery.ToQueryString();
        var result = await filteredQuery.ToListAsync();

        testOutputHelper.WriteLine("Complex Navigation Query:");
        testOutputHelper.WriteLine(sqlQuery);

        Assert.NotNull(sqlQuery);
        Assert.Contains("JOIN", sqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Single(result);
        Assert.Equal("Alice", result.First().Name);
    }

    [Fact]
    public async Task ApplyFilters_WithDateOperations_ShouldWorkWithPostgreSQLDateFunctions()
    {
        var users = Context.Users.AsQueryable();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters = [new FilterCriterion("MoneyAmount", Operator.Equals, "200")]
            }
        };

        var superfilter = new Superfilter.Superfilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "MoneyAmount", new FieldConfiguration 
                { 
                    EntityPropertyName = "MoneyAmount", 
                    Selector = (Expression<Func<User, object>>)(x => x.MoneyAmount), 
                    IsRequired = false 
                } 
            }
        };
        
        globalConfiguration.PropertyMappings = propertyMappings;
        superfilter.InitializeGlobalConfiguration(globalConfiguration);
        superfilter.InitializeFieldSelectors<User>();

        var filteredQuery = superfilter.ApplyConfiguredFilters(users);
        var sqlQuery = filteredQuery.ToQueryString();
        var result = await filteredQuery.ToListAsync();

        testOutputHelper.WriteLine("Basic Filter Query:");
        testOutputHelper.WriteLine(sqlQuery);

        Assert.NotNull(sqlQuery);
        Assert.Contains("WHERE", sqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Single(result);
        Assert.Equal("Bob", result.First().Name);
        Assert.Equal(200, result.First().MoneyAmount);
    }

    [Fact]
    public async Task ApplyFilters_WithStringContains_ShouldUsePostgreSQLILike()
    {
        var users = Context.Users.AsQueryable();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters = [new FilterCriterion("name", Operator.Contains, "li")]
            }
        };

        var superfilter = new Superfilter.Superfilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "name", new FieldConfiguration 
                { 
                    EntityPropertyName = "Name", 
                    Selector = (Expression<Func<User, object>>)(x => x.Name), 
                    IsRequired = false 
                } 
            }
        };
        
        globalConfiguration.PropertyMappings = propertyMappings;
        superfilter.InitializeGlobalConfiguration(globalConfiguration);
        superfilter.InitializeFieldSelectors<User>();

        var filteredQuery = superfilter.ApplyConfiguredFilters(users);
        var sqlQuery = filteredQuery.ToQueryString();
        var result = await filteredQuery.ToListAsync();

        testOutputHelper.WriteLine("String Contains Query:");
        testOutputHelper.WriteLine(sqlQuery);

        Assert.NotNull(sqlQuery);
        Assert.Equal(2, result.Count); // Alice and Charlie
        Assert.Contains(result, u => u.Name == "Alice");
        Assert.Contains(result, u => u.Name == "Charlie");
    }

    [Fact]
    public async Task ApplyFilters_WithMultipleComplexFilters_ShouldPerformWellOnPostgreSQL()
    {
        var users = Context.Users
            .Include(u => u.Car)
            .ThenInclude(c => c!.Brand)
            .AsQueryable();

        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters = 
                [
                    new FilterCriterion("MoneyAmount", Operator.GreaterThan, "75"),
                    new FilterCriterion("carName", Operator.Contains, "Ford"),
                    new FilterCriterion("brandRate", Operator.GreaterThan, "3")
                ]
            }
        };

        var superfilter = new Superfilter.Superfilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "MoneyAmount", new FieldConfiguration 
                { 
                    EntityPropertyName = "MoneyAmount", 
                    Selector = (Expression<Func<User, object>>)(x => x.MoneyAmount), 
                    IsRequired = false 
                } 
            },
            { "carName", new FieldConfiguration 
                { 
                    EntityPropertyName = "CarName", 
                    Selector = (Expression<Func<User, object>>)(x => x.Car!.Name), 
                    IsRequired = false 
                } 
            },
            { "brandRate", new FieldConfiguration 
                { 
                    EntityPropertyName = "BrandRate", 
                    Selector = (Expression<Func<User, object>>)(x => x.Car!.Brand!.Rate), 
                    IsRequired = false 
                } 
            }
        };
        
        globalConfiguration.PropertyMappings = propertyMappings;
        superfilter.InitializeGlobalConfiguration(globalConfiguration);
        superfilter.InitializeFieldSelectors<User>();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var filteredQuery = superfilter.ApplyConfiguredFilters(users);
        var sqlQuery = filteredQuery.ToQueryString();
        var result = await filteredQuery.ToListAsync();
        stopwatch.Stop();

        testOutputHelper.WriteLine("Complex Multi-Filter Query:");
        testOutputHelper.WriteLine(sqlQuery);
        testOutputHelper.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds}ms");

        Assert.NotNull(sqlQuery);
        Assert.Contains("JOIN", sqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Single(result);
        Assert.Equal("Alice", result.First().Name);
        Assert.True(stopwatch.ElapsedMilliseconds < 1000); // Performance check
    }
}