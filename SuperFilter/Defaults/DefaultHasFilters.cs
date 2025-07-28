using Superfilter.Entities;

namespace Superfilter.Defaults;

public class DefaultHasFilters(List<FilterCriterion> filters) : IHasFilters, IHasPagination
{
    public List<FilterCriterion> Filters { get; set; } = filters;
    public Pagination Pagination { get; set; } = new (1, 10);
}