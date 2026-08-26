namespace Ehsms.BuildingBlocks;

public interface IPaginatedRequest
{
    int Page { get; set; }
    int PageSize { get; set; }
    string? SortBy { get; set; }
    bool Descending { get; set; }
}

public class PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
