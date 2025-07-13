namespace Superfilter.Entities;

public class DefaultHasSorts(List<SortCriterion> sorters) : IHasSorts
{
    public List<SortCriterion> Sorters { get; set; } = sorters;
}