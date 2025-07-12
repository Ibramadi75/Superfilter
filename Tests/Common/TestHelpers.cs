using Superfilter.Entities;

namespace Tests.Common;

public class HasFiltersDto : IHasFilters
{
    public List<FilterCriterion> Filters { get; set; } = [];
}

public class HasSortsDto : IHasSorts
{
    public List<SortCriterion> Sorters { get; set; } = [];
}