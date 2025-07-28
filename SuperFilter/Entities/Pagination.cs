namespace Superfilter.Entities;

public record Pagination(int PageNumber, int PageSize)
{
    public int Skip => (PageNumber - 1) * PageSize;
    public int Take => PageSize;

    public static Pagination Default => new(1, 10);
}