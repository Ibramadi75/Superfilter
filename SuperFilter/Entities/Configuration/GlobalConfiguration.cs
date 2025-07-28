using Superfilter.Defaults;

namespace Superfilter.Entities;

public class GlobalConfiguration
{
    public Dictionary<string, FieldConfiguration> PropertyMappings { get; init; } = new();
    public OnErrorStrategy MissingOnStrategy { get; set; } = OnErrorStrategy.ThrowException;
    public IHasFilters HasFilters { get; init; } = new DefaultHasFilters([]);
    public IHasSorts HasSorts { get; init; } = new DefaultHasSorts([]);
}