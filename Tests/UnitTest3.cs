using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using SuperFilter;
using Database.Models;
using Xunit.Abstractions;

public class ApplyFiltersTranslationTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ApplyFiltersTranslationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private DbContextOptions<AppDbContext> GetDbContextOptions()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        return new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
    }

    private AppDbContext GetDbContext()
    {
        var context = new AppDbContext(GetDbContextOptions());
        context.Database.EnsureCreated(); // Important pour SQLite In-Memory

        context.Users.AddRange(
            new User { Id = 1, Name = "Alice", MoneyAmount = 150, Car = new Car { Id = 1, Name = "Ford" }},
            new User { Id = 2, Name = "Bob", MoneyAmount = 200, Car = new Car { Id = 2, Name = "Fiat" }},
            new User { Id = 3, Name = "Charlie", MoneyAmount = 50, Car = new Car { Id = 3, Name = "BMW" }},
            new User { Id = 4, Name = "Dave", MoneyAmount = 300, Car = new Car { Id = 4, Name = "Honda" }}
        );
        context.SaveChanges();
        return context;
    }

    [Fact]
    public void ApplyFilters_ShouldGenerateValidSqlQuery()
    {
        // Arrange
        using var context = GetDbContext();
        var users = context.Users.AsQueryable();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("MoneyAmount", Operator.GreaterThan, "100"),
                ]
            }
        };

        var superFilter = new SuperFilter.SuperFilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "MoneyAmount", new FieldConfiguration { DtoPropertyName = "MoneyAmount", Selector = (Expression<Func<User, object>>)(x => x.MoneyAmount), IsRequired = false } },
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Act
        var filteredQuery = superFilter.ApplyFilters(users);
        var sqlQuery = filteredQuery.ToQueryString();

        _testOutputHelper.WriteLine(sqlQuery);

        // Assert SQL query validity
        Assert.NotNull(sqlQuery);
        Assert.Contains("SELECT", sqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE", sqlQuery, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplyFilters_ShouldReturnExpectedResults()
    {
        // Arrange
        using var context = GetDbContext();
        var users = context.Users.AsQueryable();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("MoneyAmount", Operator.GreaterThan, "100"),
                ]
            }
        };

        var superFilter = new SuperFilter.SuperFilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "MoneyAmount", new FieldConfiguration { DtoPropertyName = "MoneyAmount", Selector = (Expression<Func<User, object>>)(x => x.MoneyAmount), IsRequired = false } },
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Act
        var filteredQuery = superFilter.ApplyFilters(users);
        var sqlQuery = filteredQuery.ToQueryString(); // Voir la requête générée
        var result = filteredQuery.ToList(); // Exécuter la requête

        _testOutputHelper.WriteLine(sqlQuery);

        // Assert SQL query validity
        Assert.NotNull(sqlQuery);
        Assert.Contains("SELECT", sqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE", sqlQuery, StringComparison.OrdinalIgnoreCase);

        // Assert query execution
        Assert.NotEmpty(result); // Vérifier que des utilisateurs sont retournés
        Assert.All(result, user => Assert.True(user.MoneyAmount > 100)); // Vérifier que le filtre est respecté
    }

    [Fact]
    public void ApplyFilters_WithNavigationProperty_ShouldFilterCorrectly()
    {
        // Arrange
        using var context = GetDbContext();
        var users = context.Users.AsQueryable();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("CarName", Operator.StartsWith, "F"),
                ]
            }
        };

        var superFilter = new SuperFilter.SuperFilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "CarName", new FieldConfiguration { DtoPropertyName = "CarName", Selector = (Expression<Func<User, object>>)(x => x.Car.Name), IsRequired = false } },
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Act
        var filteredQuery = superFilter.ApplyFilters(users);
        var sqlQuery = filteredQuery.ToQueryString(); // Voir la requête générée
        var result = filteredQuery.ToList(); // Exécuter la requête

        _testOutputHelper.WriteLine(sqlQuery);

        // Assert SQL query validity
        Assert.NotNull(sqlQuery);
        Assert.Contains("SELECT", sqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE", sqlQuery, StringComparison.OrdinalIgnoreCase);

        // Assert query execution
        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Car.Name == "Ford");
        Assert.Contains(result, u => u.Car.Name == "Fiat");
    }

    [Fact]
    public void ApplyFilters_WithMultipleFilters_ShouldApplyAllFilters()
    {
        // Arrange
        using var context = GetDbContext();
        var users = context.Users.AsQueryable();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("MoneyAmount", Operator.GreaterThan, "100"),
                    new FilterCriterion("Name", Operator.Contains, "a"),
                ]
            }
        };

        var superFilter = new SuperFilter.SuperFilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "MoneyAmount", new FieldConfiguration { DtoPropertyName = "MoneyAmount", Selector = (Expression<Func<User, object>>)(x => x.MoneyAmount), IsRequired = false } },
            { "Name", new FieldConfiguration { DtoPropertyName = "Name", Selector = (Expression<Func<User, object>>)(x => x.Name), IsRequired = false } },
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Act
        var filteredQuery = superFilter.ApplyFilters(users);
        var sqlQuery = filteredQuery.ToQueryString(); // Voir la requête générée
        var result = filteredQuery.ToList(); // Exécuter la requête

        _testOutputHelper.WriteLine(sqlQuery);

        // Assert SQL query validity
        Assert.NotNull(sqlQuery);
        Assert.Contains("SELECT", sqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE", sqlQuery, StringComparison.OrdinalIgnoreCase);

        // Assert query execution
        Assert.Equal(2, result.Count);
        Assert.All(result, user => Assert.True(user.MoneyAmount > 100));
        Assert.All(result, user => Assert.Contains("a", user.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ApplyFilters_WithInvalidOperator_ShouldThrowException()
    {
        // Arrange
        using var context = GetDbContext();
        var users = context.Users.AsQueryable();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("name", Operator.GreaterThan, "test"), // Invalid operator for string
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

        // Assert query execution
        Assert.Throws<SuperFilterException>(() => superFilter.ApplyFilters(users));
    }

    [Fact]
    public void ApplyFilters_WithEmptyFilter_ShouldReturnAllResults()
    {
        // Arrange
        using var context = GetDbContext();
        var users = context.Users.AsQueryable();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("name", Operator.Contains, "e"),
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

        // Act
        var filteredQuery = superFilter.ApplyFilters(users);
        var sqlQuery = filteredQuery.ToQueryString(); // Voir la requête générée
        var result = filteredQuery.ToList(); // Exécuter la requête

        _testOutputHelper.WriteLine(sqlQuery);

        // Assert SQL query validity
        Assert.NotNull(sqlQuery);
        Assert.Contains("SELECT", sqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE", sqlQuery, StringComparison.OrdinalIgnoreCase);

        // Assert query execution
        Assert.Equal(3, result.Count); // Should return all users
    }

    [Fact]
    public void ApplyFilters_WithNonExistentProperty_ShouldIgnoreFilter()
    {
        // Arrange
        using var context = GetDbContext();
        var users = context.Users.AsQueryable();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("NonExistentProperty", Operator.Equals, "test"),
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

        // Act
        var filteredQuery = superFilter.ApplyFilters(users);
        var sqlQuery = filteredQuery.ToQueryString(); // Voir la requête générée
        var result = filteredQuery.ToList(); // Exécuter la requête

        _testOutputHelper.WriteLine(sqlQuery);

        // Assert SQL query validity
        Assert.NotNull(sqlQuery);
        Assert.Contains("SELECT", sqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE", sqlQuery, StringComparison.OrdinalIgnoreCase);

        // Assert query execution
        Assert.Equal(4, result.Count); // Should return all users
    }

    [Fact]
    public void ApplyFilters_WithCaseInsensitivePropertyName_ShouldFilterCorrectly()
    {
        // Arrange
        using var context = GetDbContext();
        var users = context.Users.AsQueryable();
        var globalConfiguration = new GlobalConfiguration
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("moneyamount", Operator.GreaterThan, "200"), // lowercase property name
                ]
            }
        };

        var superFilter = new SuperFilter.SuperFilter();
        var propertyMappings = new Dictionary<string, FieldConfiguration>
        {
            { "moneyamount", new FieldConfiguration { DtoPropertyName = "moneyamount", Selector = (Expression<Func<User, object>>)(x => x.MoneyAmount), IsRequired = false } },
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Act
        var filteredQuery = superFilter.ApplyFilters(users);
        var sqlQuery = filteredQuery.ToQueryString(); // Voir la requête générée
        var result = filteredQuery.ToList(); // Exécuter la requête

        _testOutputHelper.WriteLine(sqlQuery);

        // Assert SQL query validity
        Assert.NotNull(sqlQuery);
        Assert.Contains("SELECT", sqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE", sqlQuery, StringComparison.OrdinalIgnoreCase);

        // Assert query execution
        Assert.Single(result);
        Assert.All(result, user => Assert.True(user.MoneyAmount > 200));
    }
}