namespace Superfilter.Entities;

public interface IHasFilters
{
    public List<FilterCriterion> Filters { get; set; }
}