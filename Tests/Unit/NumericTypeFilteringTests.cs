using Database.Models;
using Superfilter;
using Superfilter.Constants;
using Superfilter.Entities;
using Tests.Common;

namespace Tests.Unit;

public class NumericTypeFilteringTests
{
    private static IQueryable<User> GetTestUsers()
    {
        return new List<User>
        {
            new()
            {
                Id = 1, Name = "Alice",
                LongValue = 1000000000000L,
                NullableLongValue = 2000000000000L,
                DecimalValue = 123.456m,
                NullableDecimalValue = 789.012m,
                DoubleValue = 3.14159,
                NullableDoubleValue = 2.71828,
                FloatValue = 1.5f,
                NullableFloatValue = 2.5f
            },
            new()
            {
                Id = 2, Name = "Bob",
                LongValue = 3000000000000L,
                NullableLongValue = null,
                DecimalValue = 456.789m,
                NullableDecimalValue = null,
                DoubleValue = 6.28318,
                NullableDoubleValue = null,
                FloatValue = 3.5f,
                NullableFloatValue = null
            },
            new()
            {
                Id = 3, Name = "Charlie",
                LongValue = 500000000000L,
                NullableLongValue = 1500000000000L,
                DecimalValue = 999.999m,
                NullableDecimalValue = 111.111m,
                DoubleValue = 1.41421,
                NullableDoubleValue = 1.73205,
                FloatValue = 0.5f,
                NullableFloatValue = 4.5f
            }
        }.AsQueryable();
    }

    #region Long Tests

    [Fact]
    public void FilterProperty_OnLong_EqualsOperator_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("longValue", Operator.Equals, "1000000000000")]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("longValue", x => x.LongValue)
            .WithFilters(filters).ToList();

        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    [Fact]
    public void FilterProperty_OnLong_LessThanOperator_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("longValue", Operator.LessThan, "2000000000000")]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("longValue", x => x.LongValue)
            .WithFilters(filters).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Name == "Alice");
        Assert.Contains(result, u => u.Name == "Charlie");
    }

    [Fact]
    public void FilterProperty_OnLong_GreaterThanOperator_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("longValue", Operator.GreaterThan, "1000000000000")]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("longValue", x => x.LongValue)
            .WithFilters(filters).ToList();

        Assert.Single(result);
        Assert.Equal("Bob", result[0].Name);
    }

    [Fact]
    public void FilterProperty_OnNullableLong_EqualsOperator_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("nullableLongValue", Operator.Equals, "2000000000000")]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("nullableLongValue", x => x.NullableLongValue)
            .WithFilters(filters).ToList();

        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    #endregion

    #region Decimal Tests

    [Fact]
    public void FilterProperty_OnDecimal_EqualsOperator_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("decimalValue", Operator.Equals, "123.456")]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("decimalValue", x => x.DecimalValue)
            .WithFilters(filters).ToList();

        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    [Fact]
    public void FilterProperty_OnDecimal_LessThanOperator_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("decimalValue", Operator.LessThan, "500")]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("decimalValue", x => x.DecimalValue)
            .WithFilters(filters).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Name == "Alice");
        Assert.Contains(result, u => u.Name == "Bob");
    }

    [Fact]
    public void FilterProperty_OnDecimal_GreaterThanOperator_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("decimalValue", Operator.GreaterThan, "500")]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("decimalValue", x => x.DecimalValue)
            .WithFilters(filters).ToList();

        Assert.Single(result);
        Assert.Equal("Charlie", result[0].Name);
    }

    [Fact]
    public void FilterProperty_OnNullableDecimal_EqualsOperator_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("nullableDecimalValue", Operator.Equals, "789.012")]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("nullableDecimalValue", x => x.NullableDecimalValue)
            .WithFilters(filters).ToList();

        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    #endregion

    #region Double Tests

    [Fact]
    public void FilterProperty_OnDouble_EqualsOperator_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("doubleValue", Operator.Equals, "3.14159")]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("doubleValue", x => x.DoubleValue)
            .WithFilters(filters).ToList();

        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    [Fact]
    public void FilterProperty_OnDouble_LessThanOperator_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("doubleValue", Operator.LessThan, "5")]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("doubleValue", x => x.DoubleValue)
            .WithFilters(filters).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Name == "Alice");
        Assert.Contains(result, u => u.Name == "Charlie");
    }

    [Fact]
    public void FilterProperty_OnDouble_GreaterThanOperator_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("doubleValue", Operator.GreaterThan, "5")]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("doubleValue", x => x.DoubleValue)
            .WithFilters(filters).ToList();

        Assert.Single(result);
        Assert.Equal("Bob", result[0].Name);
    }

    [Fact]
    public void FilterProperty_OnNullableDouble_EqualsOperator_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("nullableDoubleValue", Operator.Equals, "2.71828")]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("nullableDoubleValue", x => x.NullableDoubleValue)
            .WithFilters(filters).ToList();

        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    #endregion

    #region Float Tests

    [Fact]
    public void FilterProperty_OnFloat_EqualsOperator_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("floatValue", Operator.Equals, "1.5")]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("floatValue", x => x.FloatValue)
            .WithFilters(filters).ToList();

        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    [Fact]
    public void FilterProperty_OnFloat_LessThanOperator_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("floatValue", Operator.LessThan, "2")]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("floatValue", x => x.FloatValue)
            .WithFilters(filters).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Name == "Alice");
        Assert.Contains(result, u => u.Name == "Charlie");
    }

    [Fact]
    public void FilterProperty_OnFloat_GreaterThanOperator_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("floatValue", Operator.GreaterThan, "2")]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("floatValue", x => x.FloatValue)
            .WithFilters(filters).ToList();

        Assert.Single(result);
        Assert.Equal("Bob", result[0].Name);
    }

    [Fact]
    public void FilterProperty_OnNullableFloat_EqualsOperator_ApplyFilterCorrectly()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("nullableFloatValue", Operator.Equals, "2.5")]
        };

        List<User> result = users.WithSuperfilter()
            .MapProperty("nullableFloatValue", x => x.NullableFloatValue)
            .WithFilters(filters).ToList();

        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void FilterProperty_OnLong_InvalidFormat_ThrowsFormatException()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("longValue", Operator.Equals, "not_a_long")]
        };

        SuperfilterException exception = Assert.Throws<SuperfilterException>(() =>
            users.WithSuperfilter()
                .MapProperty("longValue", x => x.LongValue)
                .WithFilters(filters).ToList());
        Assert.Contains("Invalid long format", exception.InnerException?.InnerException?.Message);
    }

    [Fact]
    public void FilterProperty_OnDecimal_InvalidFormat_ThrowsFormatException()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("decimalValue", Operator.Equals, "not_a_decimal")]
        };

        SuperfilterException exception = Assert.Throws<SuperfilterException>(() =>
            users.WithSuperfilter()
                .MapProperty("decimalValue", x => x.DecimalValue)
                .WithFilters(filters).ToList());
        Assert.Contains("Invalid decimal format", exception.InnerException?.InnerException?.Message);
    }

    [Fact]
    public void FilterProperty_OnDouble_InvalidFormat_ThrowsFormatException()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("doubleValue", Operator.Equals, "not_a_double")]
        };

        SuperfilterException exception = Assert.Throws<SuperfilterException>(() =>
            users.WithSuperfilter()
                .MapProperty("doubleValue", x => x.DoubleValue)
                .WithFilters(filters).ToList());
        Assert.Contains("Invalid double format", exception.InnerException?.InnerException?.Message);
    }

    [Fact]
    public void FilterProperty_OnFloat_InvalidFormat_ThrowsFormatException()
    {
        IQueryable<User> users = GetTestUsers();

        HasFiltersDto filters = new()
        {
            Filters = [new FilterCriterion("floatValue", Operator.Equals, "not_a_float")]
        };

        SuperfilterException exception = Assert.Throws<SuperfilterException>(() =>
            users.WithSuperfilter()
                .MapProperty("floatValue", x => x.FloatValue)
                .WithFilters(filters).ToList());
        Assert.Contains("Invalid float format", exception.InnerException?.InnerException?.Message);
    }

    #endregion
}