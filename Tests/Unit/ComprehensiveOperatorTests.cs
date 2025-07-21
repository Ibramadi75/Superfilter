using System.Globalization;
using System.Linq;
using Database.Models;
using Superfilter;
using Superfilter.Constants;
using Superfilter.Entities;
using Tests.Common;

namespace Tests.Unit;

public class ComprehensiveOperatorTests
{
    private static IQueryable<User> GetTestUsers()
    {
        Brand brandFord = new() { Id = 1, Name = "Ford", Rate = 5 };
        Brand brandFiat = new() { Id = 2, Name = "Fiat", Rate = 3 };
        Brand brandBmw = new() { Id = 3, Name = "BMW", Rate = 4 };
        Brand brandHonda = new() { Id = 4, Name = "Honda", Rate = 2 };

        return new List<User>
        {
            new()
            {
                Id = 1, Name = "Alice", MoneyAmount = 100, 
                BornDate = DateTime.ParseExact("01/01/2000", "dd/MM/yyyy", CultureInfo.InvariantCulture),
                LongValue = 1000L, DecimalValue = 100.5m, DoubleValue = 100.75, FloatValue = 100.25f,
                NullableLongValue = 1100L, NullableDecimalValue = 100.6m, NullableDoubleValue = 100.85, NullableFloatValue = 100.35f,
                Car = new Car { Id = 1, Name = "Ford Fiesta", Brand = brandFord }
            },
            new()
            {
                Id = 2, Name = "Bob", MoneyAmount = 200, 
                BornDate = DateTime.ParseExact("15/06/1995", "dd/MM/yyyy", CultureInfo.InvariantCulture),
                LongValue = 2000L, DecimalValue = 200.5m, DoubleValue = 200.75, FloatValue = 200.25f,
                NullableLongValue = 2200L, NullableDecimalValue = 200.6m, NullableDoubleValue = 200.85, NullableFloatValue = 200.35f,
                Car = new Car { Id = 2, Name = "Fiat 500", Brand = brandFiat }
            },
            new()
            {
                Id = 3, Name = "Charlie", MoneyAmount = 300, 
                BornDate = DateTime.ParseExact("10/12/1990", "dd/MM/yyyy", CultureInfo.InvariantCulture),
                LongValue = 3000L, DecimalValue = 300.5m, DoubleValue = 300.75, FloatValue = 300.25f,
                NullableLongValue = 3300L, NullableDecimalValue = 300.6m, NullableDoubleValue = 300.85, NullableFloatValue = 300.35f,
                Car = new Car { Id = 3, Name = "BMW X3", Brand = brandBmw }
            },
            new()
            {
                Id = 4, Name = "David", MoneyAmount = 150, 
                BornDate = null,
                LongValue = 1500L, DecimalValue = 150.5m, DoubleValue = 150.75, FloatValue = 150.25f,
                NullableLongValue = null, NullableDecimalValue = null, NullableDoubleValue = null, NullableFloatValue = null,
                Car = new Car { Id = 4, Name = "Honda Civic", Brand = brandHonda }
            },
            new()
            {
                Id = 5, Name = "", MoneyAmount = 250, 
                BornDate = DateTime.ParseExact("25/08/2005", "dd/MM/yyyy", CultureInfo.InvariantCulture),
                LongValue = 2500L, DecimalValue = 250.5m, DoubleValue = 250.75, FloatValue = 250.25f,
                NullableLongValue = 2750L, NullableDecimalValue = 250.6m, NullableDoubleValue = 250.85, NullableFloatValue = 250.35f,
                Car = null
            }
        }.AsQueryable();
    }

    #region String Operators Tests

    [Fact]
    public void StringOperator_Equals_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("name", Operator.Equals, "Alice")]
        };

        var result = GetFilteredUsers(users, filters, u => u.Name);

        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    [Fact]
    public void StringOperator_NotEquals_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("name", Operator.NotEquals, "Alice")]
        };

        var result = GetFilteredUsers(users, filters, u => u.Name);

        Assert.Equal(4, result.Count);
        Assert.DoesNotContain(result, u => u.Name == "Alice");
    }

    [Fact]
    public void StringOperator_StartsWith_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("name", Operator.StartsWith, "B")]
        };

        var result = GetFilteredUsers(users, filters, u => u.Name);

        Assert.Single(result);
        Assert.Equal("Bob", result[0].Name);
    }

    [Fact]
    public void StringOperator_EndsWith_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("name", Operator.EndsWith, "e")]
        };

        var result = GetFilteredUsers(users, filters, u => u.Name);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Name == "Alice");
        Assert.Contains(result, u => u.Name == "Charlie");
    }

    [Fact]
    public void StringOperator_Contains_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("name", Operator.Contains, "a")]
        };

        var result = GetFilteredUsers(users, filters, u => u.Name);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Name == "David");
        Assert.Contains(result, u => u.Name == "Charlie");
    }

    [Fact]
    public void StringOperator_NotContains_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("name", Operator.NotContains, "a")]
        };

        var result = GetFilteredUsers(users, filters, u => u.Name);

        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, u => u.Name == "David");
        Assert.DoesNotContain(result, u => u.Name == "Charlie");
    }

    [Fact]
    public void StringOperator_IsEmpty_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("name", Operator.IsEmpty, "")]
        };

        var result = GetFilteredUsers(users, filters, u => u.Name);

        Assert.Single(result);
        Assert.Equal("", result[0].Name);
    }

    [Fact]
    public void StringOperator_IsNotEmpty_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("name", Operator.IsNotEmpty, "")]
        };

        var result = GetFilteredUsers(users, filters, u => u.Name);

        Assert.Equal(4, result.Count);
        Assert.DoesNotContain(result, u => u.Name == "");
    }

    [Fact]
    public void StringOperator_In_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("name", Operator.In, "Alice,Bob,Charlie")]
        };

        var result = GetFilteredUsers(users, filters, u => u.Name);

        Assert.Equal(3, result.Count);
        Assert.Contains(result, u => u.Name == "Alice");
        Assert.Contains(result, u => u.Name == "Bob");
        Assert.Contains(result, u => u.Name == "Charlie");
    }

    [Fact]
    public void StringOperator_NotIn_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("name", Operator.NotIn, "Alice,Bob")]
        };

        var result = GetFilteredUsers(users, filters, u => u.Name);

        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, u => u.Name == "Alice");
        Assert.DoesNotContain(result, u => u.Name == "Bob");
    }

    #endregion

    #region Integer Operators Tests

    [Fact]
    public void IntegerOperator_Equals_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.Equals, "200")]
        };

        var result = GetFilteredUsers(users, filters, u => u.MoneyAmount);

        Assert.Single(result);
        Assert.Equal(200, result[0].MoneyAmount);
    }

    [Fact]
    public void IntegerOperator_NotEquals_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.NotEquals, "200")]
        };

        var result = GetFilteredUsers(users, filters, u => u.MoneyAmount);

        Assert.Equal(4, result.Count);
        Assert.DoesNotContain(result, u => u.MoneyAmount == 200);
    }

    [Fact]
    public void IntegerOperator_LessThan_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.LessThan, "200")]
        };

        var result = GetFilteredUsers(users, filters, u => u.MoneyAmount);

        Assert.Equal(2, result.Count);
        Assert.All(result, u => Assert.True(u.MoneyAmount < 200));
    }

    [Fact]
    public void IntegerOperator_LessThanOrEqual_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.LessThanOrEqual, "200")]
        };

        var result = GetFilteredUsers(users, filters, u => u.MoneyAmount);

        Assert.Equal(3, result.Count);
        Assert.All(result, u => Assert.True(u.MoneyAmount <= 200));
    }

    [Fact]
    public void IntegerOperator_GreaterThan_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.GreaterThan, "200")]
        };

        var result = GetFilteredUsers(users, filters, u => u.MoneyAmount);

        Assert.Equal(2, result.Count);
        Assert.All(result, u => Assert.True(u.MoneyAmount > 200));
    }

    [Fact]
    public void IntegerOperator_GreaterThanOrEqual_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.GreaterThanOrEqual, "200")]
        };

        var result = GetFilteredUsers(users, filters, u => u.MoneyAmount);

        Assert.Equal(3, result.Count);
        Assert.All(result, u => Assert.True(u.MoneyAmount >= 200));
    }

    [Fact]
    public void IntegerOperator_Between_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.Between, "150,250")]
        };

        var result = GetFilteredUsers(users, filters, u => u.MoneyAmount);

        Assert.Equal(3, result.Count);
        Assert.All(result, u => Assert.True(u.MoneyAmount >= 150 && u.MoneyAmount <= 250));
    }

    [Fact]
    public void IntegerOperator_NotBetween_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.NotBetween, "150,250")]
        };

        var result = GetFilteredUsers(users, filters, u => u.MoneyAmount);

        Assert.Equal(2, result.Count);
        Assert.All(result, u => Assert.True(u.MoneyAmount < 150 || u.MoneyAmount > 250));
    }

    [Fact]
    public void IntegerOperator_In_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.In, "100,200,300")]
        };

        var result = GetFilteredUsers(users, filters, u => u.MoneyAmount);

        Assert.Equal(3, result.Count);
        Assert.All(result, u => Assert.Contains(u.MoneyAmount, new[] { 100, 200, 300 }));
    }

    [Fact]
    public void IntegerOperator_NotIn_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.NotIn, "100,200")]
        };

        var result = GetFilteredUsers(users, filters, u => u.MoneyAmount);

        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, u => u.MoneyAmount == 100);
        Assert.DoesNotContain(result, u => u.MoneyAmount == 200);
    }

    #endregion

    #region Long Operators Tests

    [Fact]
    public void LongOperator_Equals_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("longValue", Operator.Equals, "2000")]
        };

        var result = GetFilteredUsers(users, filters, u => u.LongValue);

        Assert.Single(result);
        Assert.Equal(2000L, result[0].LongValue);
    }

    [Fact]
    public void LongOperator_Between_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("longValue", Operator.Between, "1500,2500")]
        };

        var result = GetFilteredUsers(users, filters, u => u.LongValue);

        Assert.Equal(3, result.Count);
        Assert.All(result, u => Assert.True(u.LongValue >= 1500L && u.LongValue <= 2500L));
    }

    [Fact]
    public void LongOperator_In_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("longValue", Operator.In, "1000,2000,3000")]
        };

        var result = GetFilteredUsers(users, filters, u => u.LongValue);

        Assert.Equal(3, result.Count);
        Assert.All(result, u => Assert.Contains(u.LongValue, new[] { 1000L, 2000L, 3000L }));
    }

    [Fact]
    public void NullableLongOperator_IsNull_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("nullableLongValue", Operator.IsNull, "")]
        };

        var result = GetFilteredUsers(users, filters, u => u.NullableLongValue);

        Assert.Single(result);
        Assert.Null(result[0].NullableLongValue);
    }

    #endregion

    #region Decimal Operators Tests

    [Fact]
    public void DecimalOperator_Equals_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("decimalValue", Operator.Equals, "200.5")]
        };

        var result = GetFilteredUsers(users, filters, u => u.DecimalValue);

        Assert.Single(result);
        Assert.Equal(200.5m, result[0].DecimalValue);
    }

    [Fact]
    public void DecimalOperator_LessThanOrEqual_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("decimalValue", Operator.LessThanOrEqual, "200.5")]
        };

        var result = GetFilteredUsers(users, filters, u => u.DecimalValue);

        Assert.Equal(3, result.Count);
        Assert.All(result, u => Assert.True(u.DecimalValue <= 200.5m));
    }

    [Fact]
    public void DecimalOperator_Between_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("decimalValue", Operator.Between, "150.5,250.5")]
        };

        var result = GetFilteredUsers(users, filters, u => u.DecimalValue);

        Assert.Equal(3, result.Count);
        Assert.All(result, u => Assert.True(u.DecimalValue >= 150.5m && u.DecimalValue <= 250.5m));
    }

    #endregion

    #region Double Operators Tests

    [Fact]
    public void DoubleOperator_GreaterThan_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("doubleValue", Operator.GreaterThan, "200.75")]
        };

        var result = GetFilteredUsers(users, filters, u => u.DoubleValue);

        Assert.Equal(2, result.Count);
        Assert.All(result, u => Assert.True(u.DoubleValue > 200.75));
    }

    [Fact]
    public void DoubleOperator_NotEquals_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("doubleValue", Operator.NotEquals, "200.75")]
        };

        var result = GetFilteredUsers(users, filters, u => u.DoubleValue);

        Assert.Equal(4, result.Count);
        Assert.DoesNotContain(result, u => u.DoubleValue == 200.75);
    }

    #endregion

    #region Float Operators Tests

    [Fact]
    public void FloatOperator_GreaterThanOrEqual_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("floatValue", Operator.GreaterThanOrEqual, "200.25")]
        };

        var result = GetFilteredUsers(users, filters, u => u.FloatValue);

        Assert.Equal(3, result.Count);
        Assert.All(result, u => Assert.True(u.FloatValue >= 200.25f));
    }

    [Fact]
    public void FloatOperator_NotBetween_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("floatValue", Operator.NotBetween, "150.25,250.25")]
        };

        var result = GetFilteredUsers(users, filters, u => u.FloatValue);

        Assert.Equal(2, result.Count);
        Assert.All(result, u => Assert.True(u.FloatValue < 150.25f || u.FloatValue > 250.25f));
    }

    #endregion

    #region DateTime Operators Tests

    [Fact]
    public void DateTimeOperator_Equals_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("bornDate", Operator.Equals, "01/01/2000")]
        };

        var result = GetFilteredUsers(users, filters, u => u.BornDate);

        Assert.Single(result);
        Assert.Equal(new DateTime(2000, 1, 1), result[0].BornDate);
    }

    [Fact]
    public void DateTimeOperator_NotEquals_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("bornDate", Operator.NotEquals, "01/01/2000")]
        };

        var result = GetFilteredUsers(users, filters, u => u.BornDate);

        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, u => u.BornDate == new DateTime(2000, 1, 1));
    }

    [Fact]
    public void DateTimeOperator_LessThan_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("bornDate", Operator.LessThan, "01/01/2000")]
        };

        var result = GetFilteredUsers(users, filters, u => u.BornDate);

        Assert.Equal(2, result.Count);
        Assert.All(result, u => Assert.True(u.BornDate < new DateTime(2000, 1, 1)));
    }

    [Fact]
    public void DateTimeOperator_LessThanOrEqual_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("bornDate", Operator.LessThanOrEqual, "01/01/2000")]
        };

        var result = GetFilteredUsers(users, filters, u => u.BornDate);

        Assert.Equal(3, result.Count);
        Assert.All(result, u => Assert.True(u.BornDate <= new DateTime(2000, 1, 1)));
    }

    [Fact]
    public void DateTimeOperator_GreaterThan_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("bornDate", Operator.GreaterThan, "01/01/2000")]
        };

        var result = GetFilteredUsers(users, filters, u => u.BornDate);

        Assert.Single(result);
        Assert.All(result, u => Assert.True(u.BornDate > new DateTime(2000, 1, 1)));
    }

    [Fact]
    public void DateTimeOperator_GreaterThanOrEqual_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("bornDate", Operator.GreaterThanOrEqual, "01/01/2000")]
        };

        var result = GetFilteredUsers(users, filters, u => u.BornDate);

        Assert.Equal(2, result.Count);
        Assert.All(result, u => Assert.True(u.BornDate >= new DateTime(2000, 1, 1)));
    }

    [Fact]
    public void DateTimeOperator_IsBefore_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("bornDate", Operator.IsBefore, "01/01/2000")]
        };

        var result = GetFilteredUsers(users, filters, u => u.BornDate);

        Assert.Equal(2, result.Count);
        Assert.All(result, u => Assert.True(u.BornDate < new DateTime(2000, 1, 1)));
    }

    [Fact]
    public void DateTimeOperator_IsAfter_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("bornDate", Operator.IsAfter, "01/01/2000")]
        };

        var result = GetFilteredUsers(users, filters, u => u.BornDate);

        Assert.Single(result);
        Assert.All(result, u => Assert.True(u.BornDate > new DateTime(2000, 1, 1)));
    }

    [Fact]
    public void DateTimeOperator_IsEqualToYear_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("bornDate", Operator.IsEqualToYear, "01/01/2000")]
        };

        var result = GetFilteredUsers(users, filters, u => u.BornDate);

        Assert.Single(result);
        Assert.Equal(2000, result[0].BornDate?.Year);
    }

    [Fact]
    public void DateTimeOperator_IsEqualToYearAndMonth_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("bornDate", Operator.IsEqualToYearAndMonth, "01/01/2000")]
        };

        var result = GetFilteredUsers(users, filters, u => u.BornDate);

        Assert.Single(result);
        Assert.Equal(2000, result[0].BornDate?.Year);
        Assert.Equal(1, result[0].BornDate?.Month);
    }

    [Fact]
    public void DateTimeOperator_IsEqualToFullDate_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("bornDate", Operator.IsEqualToFullDate, "01/01/2000")]
        };

        var result = GetFilteredUsers(users, filters, u => u.BornDate);

        Assert.Single(result);
        Assert.Equal(new DateTime(2000, 1, 1), result[0].BornDate);
    }

    [Fact]
    public void DateTimeOperator_IsNull_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("bornDate", Operator.IsNull, "")]
        };

        var result = GetFilteredUsers(users, filters, u => u.BornDate);

        Assert.Single(result);
        Assert.Null(result[0].BornDate);
    }

    [Fact]
    public void DateTimeOperator_IsNotNull_ShouldFilterCorrectly()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("bornDate", Operator.IsNotNull, "")]
        };

        var result = GetFilteredUsers(users, filters, u => u.BornDate);

        Assert.Equal(4, result.Count);
        Assert.All(result, u => Assert.NotNull(u.BornDate));
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void IntegerOperator_Between_InvalidFormat_ShouldThrowException()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.Between, "100")]
        };

        var exception = Assert.Throws<SuperfilterException>(() => 
        {
            return users.WithSuperfilter()
                .MapProperty("moneyAmount", u => u.MoneyAmount)
                .WithFilters(filters).ToList();
        });
        Assert.IsType<ArgumentException>(exception.InnerException?.InnerException);
    }

    [Fact]
    public void IntegerOperator_InvalidValue_ShouldThrowException()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.Equals, "invalid")]
        };

        var exception = Assert.Throws<SuperfilterException>(() => 
        {
            return users.WithSuperfilter()
                .MapProperty("moneyAmount", u => u.MoneyAmount)
                .WithFilters(filters).ToList();
        });
        Assert.IsType<FormatException>(exception.InnerException?.InnerException);
    }

    [Fact]
    public void DecimalOperator_InvalidValue_ShouldThrowException()
    {
        var users = GetTestUsers();
        var filters = new HasFiltersDto
        {
            Filters = [new FilterCriterion("decimalValue", Operator.Equals, "invalid")]
        };

        var exception = Assert.Throws<SuperfilterException>(() => 
        {
            return users.WithSuperfilter()
                .MapProperty("decimalValue", u => u.DecimalValue)
                .WithFilters(filters).ToList();
        });
        Assert.IsType<FormatException>(exception.InnerException?.InnerException);
    }

    #endregion

    private List<User> GetFilteredUsers<T>(IQueryable<User> users, HasFiltersDto filters, System.Linq.Expressions.Expression<Func<User, T>> propertySelector)
    {
        return users.WithSuperfilter()
            .MapProperty(filters.Filters[0].Field, propertySelector)
            .WithFilters(filters).ToList();
    }
}