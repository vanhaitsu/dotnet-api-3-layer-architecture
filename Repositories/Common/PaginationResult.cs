namespace Repositories.Common;

public class PaginationResult<T> where T : class
{
    public int TotalCount { get; set; }
    public T? Data { get; set; }
}