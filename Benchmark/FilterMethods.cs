using Benchmark;
using Superfilter;
using Superfilter.Constants;
using Superfilter.Defaults;
using Superfilter.Entities;

namespace Benchmark;

internal static class FilterMethods
{
    public static List<User> StandardApproachMethod(List<User> users, DefaultHasFilters filters)
    {
        IQueryable<User> query = users.AsQueryable();

        if (filters.Filters.Count == 0)
            return users;

        foreach (FilterCriterion filter in filters.Filters)
            switch (filter.Field)
            {
                case "User.Name" when filter.Operator == Operator.Contains:
                    query = query.Where(u => u.Name.Contains(filter.Value));
                    break;
                // ... 
                case "User.Name":
                {
                    if (filter.Operator == Operator.In)
                    {
                        string[] values = filter.Value.Split(',');
                        query = query.Where(u => values.Contains(u.Name));
                    }

                    break;
                }
                case "User.Age":
                {
                    if (filter.Operator == Operator.GreaterThan) query = query.Where(u => u.Age > int.Parse(filter.Value));
                    // ... 
                    break;
                }
                case "User.Country":
                {
                    if (filter.Operator == Operator.In)
                    {
                        string[] values = filter.Value.Split(',');
                        query = query.Where(u => values.Contains(u.Country));
                    }
                    // ... 
                    break;
                }
            }

        return query.ToList();
    }

    public static List<User> SuperfilterWithBuilderApproachMethod(List<User> users, DefaultHasFilters filters)
    {
        IQueryable<User> query = users.AsQueryable();

        if (filters.Filters.Count == 0)
            return users;

        var result = SuperfilterBuilder.For<User>()
            .MapProperty(x => x.Name)
            .MapProperty(x => x.Age)
            .MapProperty(x => x.Country)
            .WithFilters(filters)
            .Build(query)
            .ToList();

        return result;
    }
    
    public static List<User> SuperfilterIQueryableExtensionsApproachMethod(List<User> users, DefaultHasFilters filters)
    {
        IQueryable<User> query = users.AsQueryable();

        if (filters.Filters.Count == 0)
            return users;

        var result = query
            .WithSuperfilter()
            .MapProperty(x => x.Name)
            .MapProperty(x => x.Age)
            .MapProperty(x => x.Country)
            .WithFilters(filters) // Should apply filters without waiting another method call and returning IQueryable
            .ToList();

        return result;
    }
}