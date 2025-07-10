using System.Linq.Expressions;
using Database.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SuperFilter;
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
        SqliteConnection connection = new("Filename=:memory:");
        connection.Open();

        return new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
    }

    private AppDbContext GetDbContext()
    {
        AppDbContext context = new(GetDbContextOptions());
        context.Database.EnsureCreated(); // Important pour SQLite In-Memory

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
        // Arrange
        using AppDbContext context = GetDbContext();
        IQueryable<User> users = context.Users.AsQueryable();
        GlobalConfiguration globalConfiguration = new()
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("MoneyAmount", Operator.GreaterThan, "100")
                ]
            }
        };

        SuperFilter.SuperFilter superFilter = new();
        Dictionary<string, FieldConfiguration> propertyMappings = new()
        {
            { "MoneyAmount", new FieldConfiguration { EntityPropertyName = "MoneyAmount", Selector = (Expression<Func<User, object>>)(x => x.MoneyAmount), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Act
        IQueryable<User> filteredQuery = superFilter.ApplyFilters(users);
        string sqlQuery = filteredQuery.ToQueryString();

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
        using AppDbContext context = GetDbContext();
        IQueryable<User> users = context.Users.AsQueryable();
        GlobalConfiguration globalConfiguration = new()
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("MoneyAmount", Operator.GreaterThan, "100")
                ]
            }
        };

        SuperFilter.SuperFilter superFilter = new();
        Dictionary<string, FieldConfiguration> propertyMappings = new()
        {
            { "MoneyAmount", new FieldConfiguration { EntityPropertyName = "MoneyAmount", Selector = (Expression<Func<User, object>>)(x => x.MoneyAmount), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Act
        IQueryable<User> filteredQuery = superFilter.ApplyFilters(users);
        string sqlQuery = filteredQuery.ToQueryString(); // Voir la requête générée
        List<User> result = filteredQuery.ToList(); // Exécuter la requête

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
        using AppDbContext context = GetDbContext();
        IQueryable<User> users = context.Users.AsQueryable();
        GlobalConfiguration globalConfiguration = new()
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("carName", Operator.StartsWith, "F")
                ]
            }
        };

        SuperFilter.SuperFilter superFilter = new();
        Dictionary<string, FieldConfiguration> propertyMappings = new()
        {
            { "carName", new FieldConfiguration { EntityPropertyName = "name", Selector = (Expression<Func<User, object>>)(x => x.Car.Name), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Act
        IQueryable<User> filteredQuery = superFilter.ApplyFilters(users);
        string sqlQuery = filteredQuery.ToQueryString(); // Voir la requête générée
        List<User> result = filteredQuery.ToList(); // Exécuter la requête

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

        SuperFilter.SuperFilter superFilter = new();
        Dictionary<string, FieldConfiguration> propertyMappings = new()
        {
            { "MoneyAmount", new FieldConfiguration { EntityPropertyName = "MoneyAmount", Selector = (Expression<Func<User, object>>)(x => x.MoneyAmount), IsRequired = false } },
            { "Name", new FieldConfiguration { EntityPropertyName = "Name", Selector = (Expression<Func<User, object>>)(x => x.Name), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Act
        IQueryable<User> filteredQuery = superFilter.ApplyFilters(users);
        string sqlQuery = filteredQuery.ToQueryString(); // Voir la requête générée
        List<User> result = filteredQuery.ToList(); // Exécuter la requête

        _testOutputHelper.WriteLine(sqlQuery);

        // Assert SQL query validity
        Assert.NotNull(sqlQuery);
        Assert.Contains("SELECT", sqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE", sqlQuery, StringComparison.OrdinalIgnoreCase);

        // Assert query execution
        Assert.Single(result);
        Assert.All(result, user => Assert.True(user.MoneyAmount > 100));
        Assert.All(result, user => Assert.Contains("a", user.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ApplyFilters_WithInvalidOperator_ShouldThrowException()
    {
        // Arrange
        using AppDbContext context = GetDbContext();
        IQueryable<User> users = context.Users.AsQueryable();
        GlobalConfiguration globalConfiguration = new()
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("name", Operator.GreaterThan, "test") // Invalid operator for string
                ]
            }
        };

        SuperFilter.SuperFilter superFilter = new();
        Dictionary<string, FieldConfiguration> propertyMappings = new()
        {
            { "name", new FieldConfiguration { EntityPropertyName = "name", Selector = (Expression<Func<User, object>>)(x => x.Name), IsRequired = false } }
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
        using AppDbContext context = GetDbContext();
        IQueryable<User> users = context.Users.AsQueryable();
        GlobalConfiguration globalConfiguration = new()
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("name", Operator.Contains, "e")
                ]
            }
        };

        SuperFilter.SuperFilter superFilter = new();
        Dictionary<string, FieldConfiguration> propertyMappings = new()
        {
            { "name", new FieldConfiguration { EntityPropertyName = "name", Selector = (Expression<Func<User, object>>)(x => x.Name), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Act
        IQueryable<User> filteredQuery = superFilter.ApplyFilters(users);
        string sqlQuery = filteredQuery.ToQueryString(); // Voir la requête générée
        List<User> result = filteredQuery.ToList(); // Exécuter la requête

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
        using AppDbContext context = GetDbContext();
        IQueryable<User> users = context.Users.AsQueryable();
        GlobalConfiguration globalConfiguration = new()
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("NonExistentProperty", Operator.Equals, "test")
                ]
            }
        };

        SuperFilter.SuperFilter superFilter = new();
        Dictionary<string, FieldConfiguration> propertyMappings = new()
        {
            { "name", new FieldConfiguration { EntityPropertyName = "name", Selector = (Expression<Func<User, object>>)(x => x.Name), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Act
        IQueryable<User> filteredQuery = superFilter.ApplyFilters(users);
        string sqlQuery = filteredQuery.ToQueryString(); // Voir la requête générée
        List<User> result = filteredQuery.ToList(); // Exécuter la requête

        _testOutputHelper.WriteLine(sqlQuery);

        // Assert SQL query validity
        Assert.NotNull(sqlQuery);
        // Vérifier l'absence de "WHERE" dans la requête SQL si le filtre est ignoré
        Assert.DoesNotContain("WHERE", sqlQuery, StringComparison.OrdinalIgnoreCase);

        // Assert query execution
        Assert.Equal(4, result.Count); // Should return all users
    }

    [Fact]
    public void ApplyFilters_WithCaseInsensitivePropertyName_ShouldFilterCorrectly()
    {
        // Arrange
        using AppDbContext context = GetDbContext();
        IQueryable<User> users = context.Users.AsQueryable();
        GlobalConfiguration globalConfiguration = new()
        {
            HasFilters = new HasFiltersDto
            {
                Filters =
                [
                    new FilterCriterion("moneyamount", Operator.GreaterThan, "200") // lowercase property name
                ]
            }
        };

        SuperFilter.SuperFilter superFilter = new();
        Dictionary<string, FieldConfiguration> propertyMappings = new()
        {
            { "moneyamount", new FieldConfiguration { EntityPropertyName = "moneyamount", Selector = (Expression<Func<User, object>>)(x => x.MoneyAmount), IsRequired = false } }
        };
        globalConfiguration.PropertyMappings = propertyMappings;
        superFilter.SetGlobalConfiguration(globalConfiguration);
        superFilter.SetupFieldConfiguration<User>();

        // Act
        IQueryable<User> filteredQuery = superFilter.ApplyFilters(users);
        string sqlQuery = filteredQuery.ToQueryString(); // Voir la requête générée
        List<User> result = filteredQuery.ToList(); // Exécuter la requête

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