namespace Superfilter.Entities;

public interface IHasSorts
{
    public List<SortCriterion> Sorters { get; set; }
}