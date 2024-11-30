using System.Linq.Expressions;
using Repositories.Common;
using Repositories.Entities;

namespace Repositories.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task AddAsync(T entity);
    Task AddRangeAsync(List<T> entities);
    Task<T?> GetAsync(Guid id, string? include = "");

    Task<PaginationResult<List<T>>> GetAllAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? order = null,
        string? include = "",
        int? pageIndex = null,
        int? pageSize = null
    );

    void Update(T entity);
    void UpdateRange(List<T> entities);
    void SoftRemove(T entity);
    void SoftRemoveRange(List<T> entities);
    void Restore(T entity);
    void RestoreRange(List<T> entities);
    void HardRemove(T entity);
    void HardRemoveRange(List<T> entities);
}