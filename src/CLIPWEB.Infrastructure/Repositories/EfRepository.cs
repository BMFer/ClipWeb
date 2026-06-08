using System.Linq.Expressions;
using CLIPWEB.Core.Interfaces;
using CLIPWEB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CLIPWEB.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IRepository{T}"/>.
/// </summary>
public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly ClipWebDbContext _db;
    private readonly DbSet<T> _set;

    public EfRepository(ClipWebDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _set.FindAsync([id], ct);

    public async Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default)
    {
        IQueryable<T> query = _set.AsNoTracking();
        if (predicate is not null)
            query = query.Where(predicate);
        return await query.ToListAsync(ct);
    }

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _set.AddAsync(entity, ct);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
