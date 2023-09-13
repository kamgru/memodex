namespace Memodex.WebApp.Common;

public class PagedData<TItem>
{
    public int Page { get; init; }
    public long ItemCount { get; init; }
    public int TotalPages { get; init; }
    public IReadOnlyList<TItem> Items { get; init; } = null!;
}