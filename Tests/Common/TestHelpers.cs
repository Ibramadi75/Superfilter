using Superfilter.Entities;

namespace Tests.Common;

public class HasFiltersDto(int pageNumber, int pageSize) : IHasFilters, IHasPagination
{
    public List<FilterCriterion> Filters { get; set; } = [];
    public Pagination Pagination { get; set; } = new(pageNumber, pageSize);
    public int PageNumber { get; } = pageNumber;
    public int PageSize { get; } = pageSize;
}

public class HasSortsDto : IHasSorts
{
    public List<SortCriterion> Sorters { get; set; } = [];
}