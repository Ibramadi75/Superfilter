using Superfilter.Entities;

namespace Superfilter.Defaults;

public class DefaultHasFilters(List<FilterCriterion> filters) : IHasFilters
{
    public List<FilterCriterion> Filters { get; set; } = filters;
}