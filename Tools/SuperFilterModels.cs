
using SuperFilter;

public interface IHasFilters
{
    public List<FilterCriterion> Filters { get; set; }
}

public interface IHasSorts
{
    public List<SortCriterion> Sorters { get; set; }
}

public record FilterCriterion(string Field, Operator Operator, string Value);
public record SortCriterion(string Field, string dir);
