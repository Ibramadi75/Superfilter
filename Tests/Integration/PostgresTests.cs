using System.Diagnostics;
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
        IQueryable<User> users = Context.Users.AsQueryable();

        HasFiltersDto filters = new(1, 10)
        {
            Filters = [new FilterCriterion("MoneyAmount", Operator.GreaterThan, "100")]
        };

        IQueryable<User> filteredQuery = users.WithSuperfilter()
            .MapProperty("MoneyAmount", x => x.MoneyAmount)
            .WithFilters(filters);
        string sqlQuery = filteredQuery.ToQueryString();
        List<User> result = await filteredQuery.ToListAsync();

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
        IQueryable<User> users = Context.Users
            .Include(u => u.Car)
            .ThenInclude(c => c!.Brand)
            .Include(u => u.House)
            .ThenInclude(h => h!.City)
            .AsQueryable();

        HasFiltersDto filters = new(1, 10)
        {
            Filters =
            [
                new FilterCriterion("User.Car.Brand.Name", Operator.Equals, "Ford"),
                new FilterCriterion("User.City.Name", Operator.StartsWith, "P")
            ]
        };

        IQueryable<User> filteredQuery = users.WithSuperfilter()
            .MapProperty(x => x.Car!.Brand!.Name)
            .MapProperty(x => x.House!.City.Name)
            .WithFilters(filters);
        string sqlQuery = filteredQuery.ToQueryString();
        List<User> result = await filteredQuery.ToListAsync();

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
        IQueryable<User> users = Context.Users.AsQueryable();

        HasFiltersDto filters = new(1, 10)
        {
            Filters = [new FilterCriterion("MoneyAmount", Operator.Equals, "200")]
        };

        IQueryable<User> filteredQuery = users.WithSuperfilter()
            .MapProperty("MoneyAmount", x => x.MoneyAmount)
            .WithFilters(filters);
        string sqlQuery = filteredQuery.ToQueryString();
        List<User> result = await filteredQuery.ToListAsync();

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
        IQueryable<User> users = Context.Users.AsQueryable();

        HasFiltersDto filters = new(1, 10)
        {
            Filters = [new FilterCriterion("name", Operator.Contains, "li")]
        };

        IQueryable<User> filteredQuery = users.WithSuperfilter()
            .MapProperty("name", x => x.Name)
            .WithFilters(filters);
        string sqlQuery = filteredQuery.ToQueryString();
        List<User> result = await filteredQuery.ToListAsync();

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
        IQueryable<User> users = Context.Users
            .Include(u => u.Car)
            .ThenInclude(c => c!.Brand)
            .AsQueryable();

        HasFiltersDto filters = new(1, 10)
        {
            Filters =
            [
                new FilterCriterion("User.MoneyAmount", Operator.GreaterThan, "75"),
                new FilterCriterion("User.Car.Name", Operator.Contains, "Ford"),
                new FilterCriterion("User.Car.Brand.Rate", Operator.GreaterThan, "3")
            ]
        };

        Stopwatch stopwatch = Stopwatch.StartNew();
        IQueryable<User> filteredQuery = users.WithSuperfilter()
            .MapProperty(x => x.MoneyAmount)
            .MapProperty(x => x.Car!.Name)
            .MapProperty(x => x.Car!.Brand!.Rate)
            .WithFilters(filters);
        string sqlQuery = filteredQuery.ToQueryString();
        List<User> result = await filteredQuery.ToListAsync();
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