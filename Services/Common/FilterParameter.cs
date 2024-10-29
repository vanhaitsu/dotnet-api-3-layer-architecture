using System.Text.Json.Serialization;
using Repositories.Common;

namespace Services.Common;

public class FilterParameter
{
    private int _pageSize;

    public FilterParameter()
    {
        _pageSize = MinPageSize;
    }

    #region Pagination

    protected virtual int MinPageSize { get; set; } = Constant.DEFAULT_MIN_PAGE_SIZE;
    protected virtual int MaxPageSize { get; set; } = Constant.DEFAULT_MAX_PAGE_SIZE;
    public int PageIndex { get; set; } = 1;

    [JsonIgnore]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < MinPageSize ? MinPageSize : value;
    }

    #endregion

    #region Filter

    public string? Search { get; set; }
    public string Order { get; set; } = "";
    public bool OrderByDescending { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    #endregion
}