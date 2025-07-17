using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Superfilter;
using Superfilter.Constants;
using Superfilter.Defaults;
using Superfilter.Entities;

Console.WriteLine("🔍 Test du point de basculement performance...");

BenchmarkSwitcher
    .FromAssembly(Assembly.GetExecutingAssembly())
    .Run(args);

public static class UserGenerator
{
    public static List<User> GenerateUsers(int count)
    {
        var random = new Random(42);
        var countries = new[] { "USA", "Canada", "France", "Germany", "UK", "Japan", "Australia" };
        var firstNames = new[] { "John", "Jane", "Alice", "Bob", "Charlie", "Diana", "Eva", "Frank" };
        var lastNames = new[] { "Smith", "Johnson", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor" };
        
        var users = new List<User>(count);
        
        for (int i = 0; i < count; i++)
        {
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var name = $"{firstName} {lastName} {i}";
            var age = random.Next(18, 80);
            var country = countries[random.Next(countries.Length)];
            
            users.Add(new User(name, age, country));
        }
        
        return users;
    }
}

public static class FilterMethods
{
    public static List<User> StandardApproachMethod(List<User> users, DefaultHasFilters filters)
    {
        IQueryable<User> query = users.AsQueryable();

        if (filters.Filters.Count == 0)
            return users;

        foreach (var filter in filters.Filters)
        {
            if (filter.Field == "User.Name")
            {
                if (filter.Operator == Operator.Contains)
                {
                    query = query.Where(u => u.Name.Contains(filter.Value));
                }
                else if (filter.Operator == Operator.In)
                {
                    var values = filter.Value.Split(',');
                    query = query.Where(u => values.Contains(u.Name));
                }
                // ... 
            }
            else if (filter.Field == "User.Age")
            {
                if (filter.Operator == Operator.GreaterThan)
                {
                    query = query.Where(u => u.Age > int.Parse(filter.Value));
                }
                // ... 
            }
            else if (filter.Field == "User.Country")
            {
                if (filter.Operator == Operator.In)
                {
                    var values = filter.Value.Split(',');
                    query = query.Where(u => values.Contains(u.Country));
                }
                // ... 
            }
        }

        return query.ToList();
    }

    public static List<User> SuperfilterApproachMethod(List<User> users, DefaultHasFilters filters)
    {
        IQueryable<User> query = users.AsQueryable();

        if (filters.Filters.Count == 0)
            return users;

        var superfilter = SuperfilterBuilder.For<User>()
            .MapProperty(x => x.Name)
            .MapProperty(x => x.Age)
            .MapProperty(x => x.Country)
            .WithFilters(filters)
            .Build();
        
        return superfilter.ApplyConfiguredFilters(query).ToList();
    }
}

[MemoryDiagnoser]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)] // Plus rapide pour tester de gros volumes
public class LargeScalePerformanceTest
{
    // Test jusqu'à des datasets très larges pour trouver le point de basculement
    [Params(50_000, 100_000, 250_000, 500_000, 1_000_000)]
    public int DatasetSize { get; set; }

    private List<User> _users = [];
    
    private readonly DefaultHasFilters _defaultHasFilters = new DefaultHasFilters([
        new FilterCriterion("User.Name", Operator.Contains, "John"),
        new FilterCriterion("User.Age", Operator.GreaterThan, "30"),
        new FilterCriterion("User.Country", Operator.In, "USA,Canada,France"),
    ]);

    [GlobalSetup]
    public void Setup()
    {
        Console.WriteLine($"🔄 Génération de {DatasetSize:N0} utilisateurs...");
        _users = UserGenerator.GenerateUsers(DatasetSize);
        Console.WriteLine($"✅ Dataset de {DatasetSize:N0} utilisateurs prêt");
    }

    [Benchmark]
    public List<User> StandardApproach()
    {
        return FilterMethods.StandardApproachMethod(_users, _defaultHasFilters);
    }
    
    [Benchmark]
    public List<User> SuperfilterApproach()
    {
        return FilterMethods.SuperfilterApproachMethod(_users, _defaultHasFilters);
    }
}

// Benchmark séparé pour des tests de performance ultra-haute
[MemoryDiagnoser]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)]
public class UltraLargeScaleTest
{
    // Test extrême pour voir si on peut atteindre le point de basculement
    [Params(2_000_000, 5_000_000)]
    public int DatasetSize { get; set; }

    private List<User> _users = [];
    
    private readonly DefaultHasFilters _defaultHasFilters = new DefaultHasFilters([
        new FilterCriterion("User.Name", Operator.Contains, "John"),
        new FilterCriterion("User.Age", Operator.GreaterThan, "30"),
        new FilterCriterion("User.Country", Operator.In, "USA,Canada,France"),
    ]);

    [GlobalSetup]
    public void Setup()
    {
        Console.WriteLine($"🚀 Génération de {DatasetSize:N0} utilisateurs (test extrême)...");
        _users = UserGenerator.GenerateUsers(DatasetSize);
        Console.WriteLine($"✅ Dataset extrême de {DatasetSize:N0} utilisateurs prêt");
    }

    [Benchmark]
    public List<User> StandardApproach()
    {
        return FilterMethods.StandardApproachMethod(_users, _defaultHasFilters);
    }
    
    [Benchmark]
    public List<User> SuperfilterApproach()
    {
        return FilterMethods.SuperfilterApproachMethod(_users, _defaultHasFilters);
    }
}

public record User(string Name, int Age, string Country);