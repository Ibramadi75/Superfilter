using Database.Models;
using Superfilter;
using Superfilter.Constants;
using Superfilter.Entities;
using Tests.Common;

namespace Tests.Unit;

public class PaginationTests
{
    #region DTO and Interface Tests

    [Fact]
    public void HasFiltersDto_WithPagination_ShouldHaveCorrectProperties()
    {
        HasFiltersDto request = new(2, 5)
        {
            Filters = [new FilterCriterion("name", Operator.Contains, "a")]
        };

        Assert.Equal(2, request.PageNumber);
        Assert.Equal(5, request.PageSize);
        Assert.Single(request.Filters);
    }

    [Fact]
    public void Pagination_Skip_ShouldCalculateCorrectly()
    {
        Pagination page1 = new(1, 3);
        Pagination page2 = new(2, 3);
        Pagination page3 = new(3, 3);

        Assert.Equal(0, page1.Skip); // (1-1) * 3 = 0
        Assert.Equal(3, page2.Skip); // (2-1) * 3 = 3
        Assert.Equal(6, page3.Skip); // (3-1) * 3 = 6
    }

    [Fact]
    public void Pagination_Take_ShouldReturnPageSize()
    {
        Pagination p1 = new(1, 5);
        Pagination p2 = new(2, 10);
        Pagination p3 = new(3, 20);

        Assert.Equal(5, p1.Take);
        Assert.Equal(10, p2.Take);
        Assert.Equal(20, p3.Take);
    }

    [Fact]
    public void HasFiltersDto_ShouldImplementRequiredInterfaces()
    {
        HasFiltersDto request = new(1, 10);

        Assert.IsAssignableFrom<IHasPagination>(request);
        Assert.IsAssignableFrom<IHasFilters>(request);
    }

    #endregion

    #region New Fluent API - WithSuperfilter(request) Pattern

    [Fact]
    public void FluentApi_WithFiltersAndPagination_ShouldWork()
    {
        IQueryable<User> users = new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 100 },
            new() { Id = 2, Name = "Bob", MoneyAmount = 200 },
            new() { Id = 3, Name = "Charlie", MoneyAmount = 300 },
            new() { Id = 4, Name = "Dave", MoneyAmount = 400 },
            new() { Id = 5, Name = "Eve", MoneyAmount = 500 }
        }.AsQueryable();

        HasFiltersDto request = new(2, 2) // Page 2, size 2
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.GreaterThan, "100")]
        };

        // New API: ApplyFilters() then ApplyPagination()
        List<User> result = users
            .WithSuperfilter(request)
            .MapProperty("moneyAmount", x => x.MoneyAmount)
            .ApplyFilters()
            .ApplyPagination()
            .ToList();

        // Filtered (Bob, Charlie, Dave, Eve), Page 2 size 2 = Dave, Eve
        Assert.Equal(2, result.Count);
        Assert.Equal("Dave", result[0].Name);
        Assert.Equal("Eve", result[1].Name);
    }

    [Fact]
    public void FluentApi_WithFiltersOnly_ShouldWork()
    {
        IQueryable<User> users = new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 100 },
            new() { Id = 2, Name = "Bob", MoneyAmount = 200 },
            new() { Id = 3, Name = "Charlie", MoneyAmount = 300 }
        }.AsQueryable();

        HasFiltersDto request = new(1, 10)
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.GreaterThan, "150")]
        };

        // Build() auto-applies stored filters
        List<User> result = users
            .WithSuperfilter(request)
            .MapProperty("moneyAmount", x => x.MoneyAmount)
            .ApplyFilters()
            .ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Name == "Bob");
        Assert.Contains(result, u => u.Name == "Charlie");
    }

    [Fact]
    public void FluentApi_Page1_ShouldReturnFirstPage()
    {
        IQueryable<User> users = new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 100 },
            new() { Id = 2, Name = "Bob", MoneyAmount = 200 },
            new() { Id = 3, Name = "Charlie", MoneyAmount = 300 },
            new() { Id = 4, Name = "Dave", MoneyAmount = 400 },
            new() { Id = 5, Name = "Eve", MoneyAmount = 500 }
        }.AsQueryable();

        HasFiltersDto request = new(1, 2) // Page 1, size 2
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.GreaterThan, "200")]
        };

        List<User> result = users
            .WithSuperfilter(request)
            .MapProperty("moneyAmount", x => x.MoneyAmount)
            .ApplyFilters()
            .ApplyPagination()
            .ToList();

        // Filtered (Charlie, Dave, Eve), Page 1 size 2 = Charlie, Dave
        Assert.Equal(2, result.Count);
        Assert.Equal("Charlie", result[0].Name);
        Assert.Equal("Dave", result[1].Name);
    }

    [Fact]
    public void FluentApi_LastPage_ShouldReturnRemainingItems()
    {
        IQueryable<User> users = new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 100 },
            new() { Id = 2, Name = "Bob", MoneyAmount = 200 },
            new() { Id = 3, Name = "Charlie", MoneyAmount = 300 },
            new() { Id = 4, Name = "Dave", MoneyAmount = 400 },
            new() { Id = 5, Name = "Eve", MoneyAmount = 500 }
        }.AsQueryable();

        HasFiltersDto request = new(2, 2) // Page 2, size 2
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.GreaterThan, "200")]
        };

        List<User> result = users
            .WithSuperfilter(request)
            .MapProperty("moneyAmount", x => x.MoneyAmount)
            .ApplyFilters()
            .ApplyPagination()
            .ToList();

        // Filtered (Charlie, Dave, Eve), Page 2 size 2 = Eve only
        Assert.Single(result);
        Assert.Equal("Eve", result[0].Name);
    }

    [Fact]
    public void FluentApi_MultipleFilters_ShouldApplyAll()
    {
        IQueryable<User> users = new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 100 },
            new() { Id = 2, Name = "Bob", MoneyAmount = 200 },
            new() { Id = 3, Name = "Charlie", MoneyAmount = 300 },
            new() { Id = 4, Name = "Dave", MoneyAmount = 400 },
            new() { Id = 5, Name = "Eve", MoneyAmount = 500 }
        }.AsQueryable();

        HasFiltersDto request = new(1, 10)
        {
            Filters =
            [
                new FilterCriterion("moneyAmount", Operator.GreaterThan, "150"),
                new FilterCriterion("name", Operator.Contains, "a")
            ]
        };

        List<User> result = users
            .WithSuperfilter(request)
            .MapProperty("moneyAmount", x => x.MoneyAmount)
            .MapProperty("name", x => x.Name)
            .ApplyFilters()
            .ToList();

        // MoneyAmount > 150 AND Name contains 'a' = Charlie, Dave
        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Name == "Charlie");
        Assert.Contains(result, u => u.Name == "Dave");
    }

    #endregion

    #region Offset-based Pagination

    [Fact]
    public void FluentApi_OffsetPagination_ShouldWork()
    {
        IQueryable<User> users = new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 100 },
            new() { Id = 2, Name = "Bob", MoneyAmount = 200 },
            new() { Id = 3, Name = "Charlie", MoneyAmount = 300 },
            new() { Id = 4, Name = "Dave", MoneyAmount = 400 },
            new() { Id = 5, Name = "Eve", MoneyAmount = 500 }
        }.AsQueryable();

        HasFiltersDto request = new(1, 10)
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.GreaterThan, "100")]
        };

        // Skip 1, take 2 of filtered results
        List<User> result = users
            .WithSuperfilter(request)
            .MapProperty("moneyAmount", x => x.MoneyAmount)
            .ApplyFilters()
            .ApplyOffsetPagination(skip: 1, take: 2)
            .ToList();

        // Filtered (Bob, Charlie, Dave, Eve), skip 1 take 2 = Charlie, Dave
        Assert.Equal(2, result.Count);
        Assert.Equal("Charlie", result[0].Name);
        Assert.Equal("Dave", result[1].Name);
    }

    [Fact]
    public void FluentApi_OffsetPagination_SkipAll_ShouldReturnEmpty()
    {
        IQueryable<User> users = new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 100 },
            new() { Id = 2, Name = "Bob", MoneyAmount = 200 },
            new() { Id = 3, Name = "Charlie", MoneyAmount = 300 }
        }.AsQueryable();

        HasFiltersDto request = new(1, 10)
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.GreaterThan, "100")]
        };

        List<User> result = users
            .WithSuperfilter(request)
            .MapProperty("moneyAmount", x => x.MoneyAmount)
            .ApplyFilters()
            .ApplyOffsetPagination(skip: 10, take: 5)
            .ToList();

        Assert.Empty(result);
    }

    #endregion

    #region IQueryable Extension Methods (without Superfilter)

    [Fact]
    public void IQueryable_ApplyPagination_PageBased_ShouldWork()
    {
        IQueryable<User> users = new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 100 },
            new() { Id = 2, Name = "Bob", MoneyAmount = 200 },
            new() { Id = 3, Name = "Charlie", MoneyAmount = 300 },
            new() { Id = 4, Name = "Dave", MoneyAmount = 400 },
            new() { Id = 5, Name = "Eve", MoneyAmount = 500 }
        }.AsQueryable();

        List<User> result = users
            .ApplyPagination(pageNumber: 2, pageSize: 2)
            .ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Charlie", result[0].Name);
        Assert.Equal("Dave", result[1].Name);
    }

    [Fact]
    public void IQueryable_ApplyPagination_WithIHasPagination_ShouldWork()
    {
        IQueryable<User> users = new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 100 },
            new() { Id = 2, Name = "Bob", MoneyAmount = 200 },
            new() { Id = 3, Name = "Charlie", MoneyAmount = 300 },
            new() { Id = 4, Name = "Dave", MoneyAmount = 400 },
            new() { Id = 5, Name = "Eve", MoneyAmount = 500 }
        }.AsQueryable();

        HasFiltersDto request = new(3, 2); // Page 3, size 2

        List<User> result = users
            .ApplyPagination(request)
            .ToList();

        // Page 3 size 2 = Eve only
        Assert.Single(result);
        Assert.Equal("Eve", result[0].Name);
    }

    [Fact]
    public void IQueryable_ApplyOffsetPagination_ShouldWork()
    {
        IQueryable<User> users = new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 100 },
            new() { Id = 2, Name = "Bob", MoneyAmount = 200 },
            new() { Id = 3, Name = "Charlie", MoneyAmount = 300 },
            new() { Id = 4, Name = "Dave", MoneyAmount = 400 },
            new() { Id = 5, Name = "Eve", MoneyAmount = 500 }
        }.AsQueryable();

        List<User> result = users
            .ApplyOffsetPagination(skip: 3, take: 2)
            .ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Dave", result[0].Name);
        Assert.Equal("Eve", result[1].Name);
    }

    #endregion

    #region Static Filters with Pagination

    [Fact]
    public void StaticFilters_WithPagination_ShouldWork()
    {
        IQueryable<User> users = new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 100 },
            new() { Id = 2, Name = "Bob", MoneyAmount = 200 },
            new() { Id = 3, Name = "Charlie", MoneyAmount = 300 },
            new() { Id = 4, Name = "Dave", MoneyAmount = 400 },
            new() { Id = 5, Name = "Eve", MoneyAmount = 500 }
        }.AsQueryable();

        HasFiltersDto request = new(1, 2); // Page 1, size 2

        List<User> result = users
            .WithSuperfilter(request)
            .MapProperty("moneyAmount", x => x.MoneyAmount)
            .AddStaticFilter("moneyAmount", Operator.GreaterThan, "200")
            .ApplyFilters()
            .ApplyPagination()
            .ToList();

        // Static filter (Charlie, Dave, Eve), Page 1 size 2 = Charlie, Dave
        Assert.Equal(2, result.Count);
        Assert.Equal("Charlie", result[0].Name);
        Assert.Equal("Dave", result[1].Name);
    }

    [Fact]
    public void StaticFilters_WithOffsetPagination_ShouldWork()
    {
        IQueryable<User> users = new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 100 },
            new() { Id = 2, Name = "Bob", MoneyAmount = 200 },
            new() { Id = 3, Name = "Charlie", MoneyAmount = 300 },
            new() { Id = 4, Name = "Dave", MoneyAmount = 400 },
            new() { Id = 5, Name = "Eve", MoneyAmount = 500 }
        }.AsQueryable();

        List<User> result = users
            .WithSuperfilter()
            .MapProperty("moneyAmount", x => x.MoneyAmount)
            .AddStaticFilter("moneyAmount", Operator.GreaterThan, "200")
            .ApplyFilters()
            .ApplyOffsetPagination(skip: 1, take: 1)
            .ToList();

        // Filtered (Charlie, Dave, Eve), skip 1 take 1 = Dave
        Assert.Single(result);
        Assert.Equal("Dave", result[0].Name);
    }

    #endregion

    #region Error Handling

    [Fact]
    public void ParameterlessWithFilters_WithoutRequest_ShouldThrow()
    {
        IQueryable<User> users = new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 100 }
        }.AsQueryable();

        Assert.Throws<InvalidOperationException>(() =>
            users.WithSuperfilter()
                .MapProperty("moneyAmount", x => x.MoneyAmount)
                .WithFilters()
                .ApplyFilters()
                .ToList());
    }

    [Fact]
    public void ParameterlessApplyPagination_WithoutRequest_ShouldThrow()
    {
        IQueryable<User> users = new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 100 }
        }.AsQueryable();

        Assert.Throws<InvalidOperationException>(() =>
            users.WithSuperfilter()
                .MapProperty("moneyAmount", x => x.MoneyAmount)
                .ApplyFilters()
                .ApplyPagination()
                .ToList());
    }

    #endregion
}
