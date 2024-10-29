using Repositories.Entities;

namespace Services.Common;

public class Pagination<T> where T : BaseEntity
{
    public Pagination(List<T> data, int currentPage, int pageSize, int totalPages)
    {
        Data = data;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalPages / (double)pageSize);
    }

    public List<T> Data { get; private set; }
    public int CurrentPage { get; private set; }
    public int PageSize { get; private set; }
    public int TotalPages { get; private set; }
}