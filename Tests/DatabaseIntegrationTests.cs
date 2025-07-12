using System.Linq.Expressions;
using Database.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SuperFilter;
using SuperFilter.Entities;
using Xunit.Abstractions;

namespace Tests;

public class DatabaseIntegrationTests(ITestOutputHelper testOutputHelper)
{
    private static DbContextOptions<AppDbContext> GetDbContextOptions()
    {
        SqliteConnection connection = new("Filename=:memory:");
        connection.Open();

        return new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
    }

    private static AppDbContext GetDbContext()
    {
        AppDbContext context = new(GetDbContextOptions());
        context.Database.EnsureCreated();

        context.Users.AddRange(
            new User { Id = 1, Name = "Alice", MoneyAmount = 150, Car = new Car { Id = 1, Name = "Ford" } },
            new User { Id = 2, Name = "Bob", MoneyAmount = 200, Car = new Car { Id = 2, Name = "Fiat" } },
            new User { Id = 3, Name = "Charlie", MoneyAmount = 50, Car = new Car { Id = 3, Name = "BMW" } },
            new User { Id = 4, Name = "Dave", MoneyAmount = 300, Car = new Car { Id = 4, Name = "Honda" } }
        );
        context.SaveChanges();
        return context;
    }

    [Fact]
    public void ApplyFilters_ShouldGenerateValidSqlQuery()
    {
        using AppDbContext context = GetDbContext();
        IQueryable<User> users = context.Users.AsQueryable();
        GlobalConfiguration globalConfiguration = new()
        {
            HasFilters = new HasFiltersDto
            {
                Filters = [new FilterCriterion("MoneyAmount", Operator.GreaterThan, "100")]
            }
        };

        Superfilter superfilter = new();
        Dictionary<string, FieldConfiguration> propertyMappings = new()
        {
            { "MoneyAmount", new FieldConfiguration { EntityPropertyName = "MoneyAmount", Selector = (Expression<Func<User, object>>)(x => x.MoneyAmount), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superfilter.InitializeGlobalConfiguration(globalConfiguration);
        superfilter.InitializeFieldSelectors<User>();

        IQueryable<User> filteredQuery = superfilter.ApplyConfiguredFilters(users);
        string sqlQuery = filteredQuery.ToQueryString();

        testOutputHelper.WriteLine(sqlQuery);

        Assert.NotNull(sqlQuery);
        Assert.Contains("SELECT", sqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE", sqlQuery, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplyFilters_ShouldReturnExpectedResults()
    {
        using AppDbContext context = GetDbContext();
        IQueryable<User> users = context.Users.AsQueryable();
        GlobalConfiguration globalConfiguration = new()
        {
            HasFilters = new HasFiltersDto
            {
                Filters = [new FilterCriterion("MoneyAmount", Operator.GreaterThan, "100")]
            }
        };

        Superfilter superfilter = new();
        Dictionary<string, FieldConfiguration> propertyMappings = new()
        {
            { "MoneyAmount", new FieldConfiguration { EntityPropertyName = "MoneyAmount", Selector = (Expression<Func<User, object>>)(x => x.MoneyAmount), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superfilter.InitializeGlobalConfiguration(globalConfiguration);
        superfilter.InitializeFieldSelectors<User>();

        IQueryable<User> filteredQuery = superfilter.ApplyConfiguredFilters(users);
        string sqlQuery = filteredQuery.ToQueryString();
        List<User> result = filteredQuery.ToList();

        testOutputHelper.WriteLine(sqlQuery);

        Assert.NotNull(sqlQuery);
        Assert.Contains("SELECT", sqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE", sqlQuery, StringComparison.OrdinalIgnoreCase);

        Assert.NotEmpty(result);
        Assert.All(result, user => Assert.True(user.MoneyAmount > 100));
    }

    [Fact]
    public void ApplyFilters_WithNavigationProperty_ShouldFilterCorrectly()
    {
        using AppDbContext context = GetDbContext();
        IQueryable<User> users = context.Users.AsQueryable();
        GlobalConfiguration globalConfiguration = new()
        {
            HasFilters = new HasFiltersDto
            {
                Filters = [new FilterCriterion("carName", Operator.StartsWith, "F")]
            }
        };

        Superfilter superfilter = new();
        Dictionary<string, FieldConfiguration> propertyMappings = new()
        {
            { "carName", new FieldConfiguration { EntityPropertyName = "name", Selector = (Expression<Func<User, object>>)(x => x.Car!.Name), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superfilter.InitializeGlobalConfiguration(globalConfiguration);
        superfilter.InitializeFieldSelectors<User>();

        IQueryable<User> filteredQuery = superfilter.ApplyConfiguredFilters(users);
        string sqlQuery = filteredQuery.ToQueryString();
        List<User> result = filteredQuery.ToList();

        testOutputHelper.WriteLine(sqlQuery);

        Assert.NotNull(sqlQuery);
        Assert.Contains("SELECT", sqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE", sqlQuery, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Car!.Name == "Ford");
        Assert.Contains(result, u => u.Car!.Name == "Fiat");
    }

    [Fact]
    public void ApplyFilters_WithMultipleFilters_ShouldApplyAllFilters()
    {
        using AppDbContext context = GetDbContext();
        IQueryable<User> users = context.Users.AsQueryable();
        GlobalConfiguration globalConfiguration = new()
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("MoneyAmount", Operator.GreaterThan, "100"),
                    new FilterCriterion("Name", Operator.Contains, "a")
                ]
            }
        };

        Superfilter superfilter = new();
        Dictionary<string, FieldConfiguration> propertyMappings = new()
        {
            { "MoneyAmount", new FieldConfiguration { EntityPropertyName = "MoneyAmount", Selector = (Expression<Func<User, object>>)(x => x.MoneyAmount), IsRequired = false } },
            { "Name", new FieldConfiguration { EntityPropertyName = "Name", Selector = (Expression<Func<User, object>>)(x => x.Name), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superfilter.InitializeGlobalConfiguration(globalConfiguration);
        superfilter.InitializeFieldSelectors<User>();

        IQueryable<User> filteredQuery = superfilter.ApplyConfiguredFilters(users);
        string sqlQuery = filteredQuery.ToQueryString();
        List<User> result = filteredQuery.ToList();

        testOutputHelper.WriteLine(sqlQuery);

        Assert.NotNull(sqlQuery);
        Assert.Contains("SELECT", sqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE", sqlQuery, StringComparison.OrdinalIgnoreCase);

        Assert.Single(result);
        Assert.All(result, user => Assert.True(user.MoneyAmount > 100));
        Assert.All(result, user => Assert.Contains("a", user.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ApplyFilters_WithNonExistentProperty_ShouldIgnoreFilter()
    {
        using AppDbContext context = GetDbContext();
        IQueryable<User> users = context.Users.AsQueryable();
        GlobalConfiguration globalConfiguration = new()
        {
            HasFilters = new HasFiltersDto
            {
                Filters = [new FilterCriterion("NonExistentProperty", Operator.Equals, "test")]
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

        IQueryable<User> filteredQuery = superfilter.ApplyConfiguredFilters(users);
        string sqlQuery = filteredQuery.ToQueryString();
        List<User> result = filteredQuery.ToList();

        testOutputHelper.WriteLine(sqlQuery);

        Assert.NotNull(sqlQuery);
        Assert.DoesNotContain("WHERE", sqlQuery, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void ApplyFilters_WithCaseInsensitivePropertyName_ShouldFilterCorrectly()
    {
        using AppDbContext context = GetDbContext();
        IQueryable<User> users = context.Users.AsQueryable();
        GlobalConfiguration globalConfiguration = new()
        {
            HasFilters = new HasFiltersDto
            {
                Filters = [new FilterCriterion("moneyamount", Operator.GreaterThan, "200")]
            }
        };

        Superfilter superfilter = new();
        Dictionary<string, FieldConfiguration> propertyMappings = new()
        {
            { "moneyamount", new FieldConfiguration { EntityPropertyName = "moneyamount", Selector = (Expression<Func<User, object>>)(x => x.MoneyAmount), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superfilter.InitializeGlobalConfiguration(globalConfiguration);
        superfilter.InitializeFieldSelectors<User>();

        IQueryable<User> filteredQuery = superfilter.ApplyConfiguredFilters(users);
        string sqlQuery = filteredQuery.ToQueryString();
        List<User> result = filteredQuery.ToList();

        testOutputHelper.WriteLine(sqlQuery);

        Assert.NotNull(sqlQuery);
        Assert.Contains("SELECT", sqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE", sqlQuery, StringComparison.OrdinalIgnoreCase);

        Assert.Single(result);
        Assert.All(result, user => Assert.True(user.MoneyAmount > 200));
    }
}