namespace Leaderboard.Data;

public class PagedResult<TValue>
{
    public int Count { get; set; }

    public int Page { get; set; }

    public TValue[] Values { get; set; }
}
