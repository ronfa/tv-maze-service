namespace CodingChallenge.Application.TvShow.Queries;

public class PagedList<T> where T : class
{
    public string? PaginationToken { get; set; }
    public int PageSize { get; set; }
    public List<T>? Items { get; set; }

    public PagedList(List<T> items, int pageSize, string? paginationToken = null)
    {
        PageSize = pageSize;
        PaginationToken = paginationToken;
        Items = new List<T>(items);
    }
}