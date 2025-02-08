namespace Dto;

public record FilteringDto() : IHasFilters
{
    public List<FilterCriterion> Filters { get; set; }
};