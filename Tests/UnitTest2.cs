using System.Linq;
using System.Linq.Expressions;
using Xunit;
using SuperFilter;
using Database.Models;

public class HasSortsDto : IHasSorts
{
    public List<SortCriterion> Sorters { get; set; }
}

public class SuperFilterSortTests
{
    private GlobalConfiguration GetGlobalConfiguration()
    {
        return new GlobalConfiguration
        {
            HasSorts = new HasSortsDto
            {
                Sorters =
                [
                    new SortCriterion("id", "asc"),
                    new SortCriterion("name", "asc"),
                    new SortCriterion("moneyAmount", "asc")
                ]
            }
        };
    }

    private IQueryable<User> GetTestUsers()
    {
        return new List<User>
        {
            new User { Id = 3, Name = "Charlie", MoneyAmount = 50 },
            new User { Id = 1, Name = "Alice", MoneyAmount = 150 },
            new User { Id = 4, Name = "Dave", MoneyAmount = 300 },
            new User { Id = 2, Name = "Bob", MoneyAmount = 200 }
        }.AsQueryable();
    }


}
