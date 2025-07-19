using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Superfilter.Constants;
using Superfilter.Defaults;
using Superfilter.Entities;

namespace Benchmark;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class UltraLargeScaleTest
{
    private readonly DefaultHasFilters _defaultHasFilters = new([
        new FilterCriterion("User.Name", Operator.Contains, "John"),
        new FilterCriterion("User.Age", Operator.GreaterThan, "30"),
        new FilterCriterion("User.Country", Operator.In, "USA,Canada,France")
    ]);

    private List<User> _users = [];

    // Test extrême pour voir si on peut atteindre le point de basculement
    [Params(2_000_000, 5_000_000)] public int DatasetSize { get; set; }

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
    public List<User> SuperfilterWithBuilderApproachMethod()
    {
        return FilterMethods.SuperfilterWithBuilderApproachMethod(_users, _defaultHasFilters);
    }
    
    [Benchmark]
    public List<User> SuperfilterExtensionsApproachMethod()
    {
        return FilterMethods.SuperfilterIQueryableExtensionsApproachMethod(_users, _defaultHasFilters);
    }
}