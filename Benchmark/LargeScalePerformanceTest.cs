using Benchmark;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Superfilter.Constants;
using Superfilter.Defaults;
using Superfilter.Entities;

namespace Benchmark;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)] // Plus rapide pour tester de gros volumes
public class LargeScalePerformanceTest
{
    private readonly DefaultHasFilters _defaultHasFilters = new([
        new FilterCriterion("User.Name", Operator.Contains, "John"),
        new FilterCriterion("User.Age", Operator.GreaterThan, "30"),
        new FilterCriterion("User.Country", Operator.In, "USA,Canada,France")
    ]);

    private List<User> _users = [];

    // Test jusqu'à des datasets très larges pour trouver le point de basculement
    [Params(50_000, 100_000, 250_000, 500_000, 1_000_000)]
    public int DatasetSize { get; set; }

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