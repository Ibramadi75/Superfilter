using System.Globalization;
using Database.Models;
using Superfilter;
using Superfilter.Constants;
using Superfilter.Entities;
using Tests.Common;

namespace Tests.Unit;

public class DateTimeOffsetFilteringTests
{
    private static IQueryable<User> GetTestUsers()
    {
        var baseTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);
        
        Brand brandFord = new() { Id = 1, Name = "Ford", Rate = 5 };
        Brand brandFiat = new() { Id = 2, Name = "Fiat", Rate = 3 };
        Brand brandBmw = new() { Id = 3, Name = "BMW", Rate = 4 };
        Brand brandHonda = new() { Id = 4, Name = "Honda", Rate = 2 };

        return new List<User>
        {
            new()
            {
                Id = 1, Name = "Alice", MoneyAmount = 150,
                RegistrationDate = baseTime.AddMonths(1), // 2023-02-01
                LastLoginDate = baseTime.AddDays(10), // 2023-01-11
                Car = new Car { Id = 1, Name = "Ford Fiesta", Brand = brandFord, ManufactureDate = baseTime.AddYears(-2) },
                House = new House { Id = 1, Address = "123 Main Street" }
            },
            new()
            {
                Id = 2, Name = "Bob", MoneyAmount = 200,
                RegistrationDate = baseTime.AddMonths(2), // 2023-03-01
                LastLoginDate = baseTime.AddDays(20), // 2023-01-21
                Car = new Car { Id = 2, Name = "Fiat 500", Brand = brandFiat, ManufactureDate = baseTime.AddYears(-1) },
                House = new House { Id = 2, Address = "456 Oak Street" }
            },
            new()
            {
                Id = 3, Name = "Charlie", MoneyAmount = 50,
                RegistrationDate = baseTime.AddMonths(3), // 2023-04-01
                LastLoginDate = baseTime.AddDays(30), // 2023-01-31
                Car = new Car { Id = 3, Name = "BMW X3", Brand = brandBmw, ManufactureDate = null },
                House = new House { Id = 3, Address = "789 Pine Avenue" }
            },
            new()
            {
                Id = 4, Name = "Dave", MoneyAmount = 300,
                RegistrationDate = null, // Null DateTimeOffset?
                LastLoginDate = baseTime.AddDays(40), // 2023-02-10
                Car = new Car { Id = 4, Name = "Honda Civic", Brand = brandHonda, ManufactureDate = baseTime.AddYears(-3) },
                House = new House { Id = 4, Address = "101 Maple Road" }
            }
        }.AsQueryable();
    }

    [Fact]
    public void FilterProperty_DateTimeOffset_Equals_ShouldReturnCorrectResult()
    {
        IQueryable<User> users = GetTestUsers();
        var targetDate = new DateTimeOffset(2023, 1, 11, 0, 0, 0, TimeSpan.Zero);
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("lastLoginDate", Operator.Equals, targetDate.ToString("O"))]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("lastLoginDate", x => x.LastLoginDate)
            .WithFilters(filters).ToList();

        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    [Fact]
    public void FilterProperty_DateTimeOffset_GreaterThan_ShouldReturnCorrectResult()
    {
        IQueryable<User> users = GetTestUsers();
        var targetDate = new DateTimeOffset(2023, 1, 15, 0, 0, 0, TimeSpan.Zero);
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("lastLoginDate", Operator.GreaterThan, targetDate.ToString("O"))]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("lastLoginDate", x => x.LastLoginDate)
            .WithFilters(filters).ToList();

        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, u => u.Id == 1);
    }

    [Fact]
    public void FilterProperty_DateTimeOffset_LessThan_ShouldReturnCorrectResult()
    {
        IQueryable<User> users = GetTestUsers();
        var targetDate = new DateTimeOffset(2023, 1, 25, 0, 0, 0, TimeSpan.Zero);
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("lastLoginDate", Operator.LessThan, targetDate.ToString("O"))]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("lastLoginDate", x => x.LastLoginDate)
            .WithFilters(filters).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Id == 1);
        Assert.Contains(result, u => u.Id == 2);
    }

    [Fact]
    public void FilterProperty_DateTimeOffset_IsEqualToYear_ShouldReturnCorrectResult()
    {
        IQueryable<User> users = GetTestUsers();
        var targetDate = new DateTimeOffset(2023, 5, 15, 0, 0, 0, TimeSpan.Zero);
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("lastLoginDate", Operator.IsEqualToYear, targetDate.ToString("O"))]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("lastLoginDate", x => x.LastLoginDate)
            .WithFilters(filters).ToList();

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void FilterProperty_DateTimeOffset_IsEqualToYearAndMonth_ShouldReturnCorrectResult()
    {
        IQueryable<User> users = GetTestUsers();
        var targetDate = new DateTimeOffset(2023, 1, 15, 0, 0, 0, TimeSpan.Zero);
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("lastLoginDate", Operator.IsEqualToYearAndMonth, targetDate.ToString("O"))]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("lastLoginDate", x => x.LastLoginDate)
            .WithFilters(filters).ToList();

        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, u => u.Id == 4);
    }

    [Fact]
    public void FilterProperty_DateTimeOffset_IsEqualToFullDate_ShouldReturnCorrectResult()
    {
        IQueryable<User> users = GetTestUsers();
        var targetDate = new DateTimeOffset(2023, 1, 11, 0, 0, 0, TimeSpan.Zero);
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("lastLoginDate", Operator.IsEqualToFullDate, targetDate.ToString("O"))]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("lastLoginDate", x => x.LastLoginDate)
            .WithFilters(filters).ToList();

        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    [Fact]
    public void FilterProperty_NullableDateTimeOffset_WithNullValue_ShouldHandleCorrectly()
    {
        IQueryable<User> users = GetTestUsers();
        var targetDate = new DateTimeOffset(2023, 2, 1, 0, 0, 0, TimeSpan.Zero);
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("registrationDate", Operator.Equals, targetDate.ToString("O"))]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("registrationDate", x => x.RegistrationDate!)
            .WithFilters(filters).ToList();

        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    [Fact]
    public void FilterProperty_NestedDateTimeOffset_ShouldFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();
        var targetDate = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero);
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("car.manufactureDate", Operator.Equals, targetDate.ToString("O"))]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("car.manufactureDate", x => x.Car!.ManufactureDate!)
            .WithFilters(filters).ToList();

        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    [Fact]
    public void FilterProperty_DateTimeOffsetWithInvalidFormat_ShouldThrowException()
    {
        IQueryable<User> users = GetTestUsers();
        
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("lastLoginDate", Operator.Equals, "invalid-date")]
        };

        Assert.Throws<SuperfilterException>(() => 
            users.WithSuperfilter()
                .MapProperty("lastLoginDate", x => x.LastLoginDate)
                .WithFilters(filters).ToList());
    }
}