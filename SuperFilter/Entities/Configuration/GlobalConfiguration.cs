namespace SuperFilter.Entities;

public class GlobalConfiguration
{
    public Dictionary<string, FieldConfiguration> PropertyMappings { get; set; } = new();
    public OnErrorStrategy MissingOnStrategy { get; set; } = OnErrorStrategy.ThrowException;
    public Exception StandardException { get; set; } = new SuperFilterException();
    public IHasFilters HasFilters { get; set; }
    public IHasSorts HasSorts { get; set; }
}