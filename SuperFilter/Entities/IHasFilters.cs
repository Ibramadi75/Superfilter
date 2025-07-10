namespace SuperFilter.Entities;

public interface IHasFilters
{
    public List<FilterCriterion> Filters { get; set; }
}