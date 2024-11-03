using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Repositories.Common;
using Repositories.Entities;
using Repositories.Interfaces;

namespace Repositories.Repositories;

public abstract class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly IClaimService _claimService;
    protected DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context, IClaimService claimService)
    {
        _dbSet = context.Set<T>();
        _claimService = claimService;
    }

    public virtual async Task AddAsync(T entity)
    {
        entity.CreationDate = DateTime.Now;
        entity.CreatedBy = _claimService.GetCurrentUserId;
        await _dbSet.AddAsync(entity);
    }

    public virtual async Task AddRangeAsync(List<T> entities)
    {
        foreach (var entity in entities)
        {
            entity.CreationDate = DateTime.Now;
            entity.CreatedBy = _claimService.GetCurrentUserId;
        }

        await _dbSet.AddRangeAsync(entities);
    }

    public virtual async Task<T?> GetAsync(Guid id, string? include = "")
    {
        IQueryable<T> query = _dbSet;
        if (!string.IsNullOrEmpty(include))
            foreach (var includeProperty in include.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(includeProperty.Trim());
        var result = await query.FirstOrDefaultAsync(x => x.Id == id);

        // todo: throw exception when result is not found
        return result;
    }

    public virtual async Task<PaginationResult<List<T>>> GetAllAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? order = null,
        string? include = "",
        int? pageIndex = null,
        int? pageSize = null)
    {
        IQueryable<T> query = _dbSet;

        // Filter
        if (filter != null) query = query.Where(filter);
        var totalCount = await query.CountAsync();

        // Order (sorting)
        if (order != null) query = order(query);

        // Include properties
        if (!string.IsNullOrEmpty(include))
            foreach (var includeProperty in include.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(includeProperty.Trim());

        // Pagination
        // If pageIndex and pageSize are both null, return all items
        if (pageIndex.HasValue && pageSize.HasValue)
        {
            var validPageIndex = pageIndex.Value > 0 ? pageIndex.Value - 1 : 0;
            var validPageSize = pageSize.Value > 0 ? pageSize.Value : Constant.DefaultMinPageSize;
            query = query.Skip(validPageIndex * validPageSize).Take(validPageSize);
        }

        return new PaginationResult<List<T>>
        {
            TotalCount = totalCount,
            Data = await query.ToListAsync()
        };
    }

    public virtual void Update(T entity)
    {
        entity.ModificationDate = DateTime.Now;
        entity.ModifiedBy = _claimService.GetCurrentUserId;
        _dbSet.Update(entity);
    }

    public virtual void UpdateRange(List<T> entities)
    {
        foreach (var entity in entities)
        {
            entity.ModificationDate = DateTime.Now;
            entity.ModifiedBy = _claimService.GetCurrentUserId;
        }

        _dbSet.UpdateRange(entities);
    }

    public virtual void SoftDelete(T entity)
    {
        entity.IsDeleted = true;
        entity.DeletionDate = DateTime.Now;
        entity.DeletedBy = _claimService.GetCurrentUserId;
        _dbSet.Update(entity);
    }

    public virtual void SoftDeleteRange(List<T> entities)
    {
        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
            entity.DeletionDate = DateTime.Now;
            entity.DeletedBy = _claimService.GetCurrentUserId;
        }

        _dbSet.UpdateRange(entities);
    }

    public virtual void Restore(T entity)
    {
        entity.IsDeleted = false;
        entity.DeletionDate = null;
        entity.DeletedBy = null;
        entity.ModificationDate = DateTime.Now;
        entity.ModifiedBy = _claimService.GetCurrentUserId;
        _dbSet.Update(entity);
    }

    public virtual void RestoreRange(List<T> entities)
    {
        foreach (var entity in entities)
        {
            entity.IsDeleted = false;
            entity.DeletionDate = null;
            entity.DeletedBy = null;
            entity.ModificationDate = DateTime.Now;
            entity.ModifiedBy = _claimService.GetCurrentUserId;
        }

        _dbSet.UpdateRange(entities);
    }

    public virtual void HardDelete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual void HardDeleteRange(List<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }
}