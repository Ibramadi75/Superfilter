namespace Superfilter.Entities;

public class GlobalConfiguration
{
    public Dictionary<string, FieldConfiguration> PropertyMappings { get; set; } = new();
    public OnErrorStrategy MissingOnStrategy { get; set; } = OnErrorStrategy.ThrowException;
    public required IHasFilters HasFilters { get; init; }
    public required IHasSorts HasSorts { get; init; }
}