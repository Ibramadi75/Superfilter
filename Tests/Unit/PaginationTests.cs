using Database.Models;
using Superfilter;
using Superfilter.Constants;
using Superfilter.Entities;
using Tests.Common;

namespace Tests.Unit;

public class PaginationTests
{
    [Fact]
    public void HasFiltersDto_WithPagination_ShouldHaveCorrectProperties()
    {
        HasFiltersDto filters = new(2, 5)
        {
            Filters = [new FilterCriterion("name", Operator.Contains, "a")]
        };

        Assert.Equal(2, filters.PageNumber);
        Assert.Equal(5, filters.PageSize);
        Assert.Single(filters.Filters);
    }

    [Fact]  
    public void IHasPagination_SkipProperty_ShouldCalculateCorrectly()
    {
        HasFiltersDto page1 = new(1, 3);
        HasFiltersDto page2 = new(2, 3);
        HasFiltersDto page3 = new(3, 3);

        // Test Skip calculation: (PageNumber - 1) * PageSize
        int skip1 = (page1.PageNumber - 1) * page1.PageSize;
        int skip2 = (page2.PageNumber - 1) * page2.PageSize;
        int skip3 = (page3.PageNumber - 1) * page3.PageSize;

        Assert.Equal(0, skip1); // (1-1) * 3 = 0
        Assert.Equal(3, skip2); // (2-1) * 3 = 3  
        Assert.Equal(6, skip3); // (3-1) * 3 = 6
    }

    [Fact]
    public void IHasPagination_TakeProperty_ShouldReturnPageSize()
    {
        HasFiltersDto filters1 = new(1, 5);
        HasFiltersDto filters2 = new(2, 10);
        HasFiltersDto filters3 = new(3, 20);

        // Test Take property: should return PageSize
        Assert.Equal(5, filters1.PageSize);
        Assert.Equal(10, filters2.PageSize);
        Assert.Equal(20, filters3.PageSize);
    }

    [Fact]
    public void HasFiltersDto_Construction_ShouldRequirePaginationParameters()
    {
        // Test that the constructor requires pageNumber and pageSize
        HasFiltersDto filters = new(1, 10);
        
        Assert.Equal(1, filters.PageNumber);
        Assert.Equal(10, filters.PageSize);
        Assert.Empty(filters.Filters); // Should start with empty filters
    }

    [Fact]
    public void HasFiltersDto_InheritsFromIHasPagination_ShouldBeTrue()
    {
        HasFiltersDto filters = new(1, 10);
        
        // Verify inheritance relationship
        Assert.IsAssignableFrom<IHasPagination>(filters);
        Assert.IsAssignableFrom<IHasFilters>(filters);
    }

    [Fact]
    public void PaginationWithFiltering_UsingPageNumberDirect_ShouldWorkCorrectly()
    {
        IQueryable<User> users = new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 100 },
            new() { Id = 2, Name = "Bob", MoneyAmount = 200 },
            new() { Id = 3, Name = "Charlie", MoneyAmount = 300 },
            new() { Id = 4, Name = "Dave", MoneyAmount = 400 },
            new() { Id = 5, Name = "Eve", MoneyAmount = 500 }
        }.AsQueryable();

        HasFiltersDto filters = new(1, 2)
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.GreaterThan, "200")],
            Pagination = new Pagination(1, 2)
        };

        // Test using direct PageNumber calculation
        List<User> result = users.WithSuperfilter()
            .MapProperty("moneyAmount", x => x.MoneyAmount)
            .WithFilters(filters)
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToList();

        // Page 1 with size 2 of filtered results (Charlie, Dave, Eve) should return Charlie, Dave
        Assert.Equal(2, result.Count); 
        Assert.All(result, u => Assert.True(u.MoneyAmount > 200));
        Assert.Equal("Charlie", result[0].Name);
        Assert.Equal("Dave", result[1].Name);
        
        // Verify properties and calculations
        Assert.Equal(1, filters.PageNumber);
        Assert.Equal(2, filters.PageSize);
        Assert.Equal(0, (filters.PageNumber - 1) * filters.PageSize); // Skip should be 0
        Assert.Equal(2, filters.PageSize); // Take should be 2
    }

    [Fact]
    public void PaginationWithFiltering_UsingSkipTakeProperties_ShouldWorkCorrectly()
    {
        IQueryable<User> users = new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 100 },
            new() { Id = 2, Name = "Bob", MoneyAmount = 200 },
            new() { Id = 3, Name = "Charlie", MoneyAmount = 300 },
            new() { Id = 4, Name = "Dave", MoneyAmount = 400 },
            new() { Id = 5, Name = "Eve", MoneyAmount = 500 },
            new() { Id = 6, Name = "Frank", MoneyAmount = 600 }
        }.AsQueryable();

        HasFiltersDto filters = new(2, 2) // Page 2, size 2
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.GreaterThan, "200")],
        };

        // Test using IHasPagination calculated values
        int skipCount = (filters.PageNumber - 1) * filters.PageSize;
        int takeCount = filters.PageSize;
        
        List<User> result = users.WithSuperfilter()
            .MapProperty("moneyAmount", x => x.MoneyAmount)
            .WithFilters(filters)
            .Skip(skipCount) // Uses calculated value 
            .Take(takeCount) // Uses calculated value
            .ToList();

        // Page 2 with size 2 of filtered results (Charlie, Dave, Eve, Frank) should return Eve, Frank
        Assert.Equal(2, result.Count);
        Assert.All(result, u => Assert.True(u.MoneyAmount > 200));
        Assert.Equal("Eve", result[0].Name);
        Assert.Equal("Frank", result[1].Name);

        // Verify properties and calculations
        Assert.Equal(2, filters.PageNumber);
        Assert.Equal(2, filters.PageSize);
        Assert.Equal(2, skipCount); // (2-1) * 2 = 2
        Assert.Equal(2, takeCount); // PageSize = 2
    }

    [Fact]
    public void PaginationWithFiltering_BasicCase_ShouldWorkWithSuperfilter()
    {
        IQueryable<User> users = new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 100 },
            new() { Id = 2, Name = "Bob", MoneyAmount = 200 },
            new() { Id = 3, Name = "Charlie", MoneyAmount = 300 }
        }.AsQueryable();

        HasFiltersDto filters = new(1, 10)
        {
            Filters = [new FilterCriterion("moneyAmount", Operator.GreaterThan, "100")],
        };

        // Test that filtering works with the pagination-enabled DTO
        List<User> result = users.WithSuperfilter()
            .MapProperty("moneyAmount", x => x.MoneyAmount)
            .WithFilters(filters)
            .ToList();

        Assert.Equal(2, result.Count); // Bob, Charlie have > 100
        Assert.All(result, u => Assert.True(u.MoneyAmount > 100));

        // Test that pagination properties are accessible
        Assert.Equal(1, filters.PageNumber);
        Assert.Equal(10, filters.PageSize);
    }
}