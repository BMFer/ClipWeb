using System.Linq.Expressions;

namespace CLIPWEB.Core.Interfaces;

/// <summary>
/// Generic persistence abstraction for an aggregate type.
/// Implemented in the Infrastructure layer (EF Core).
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default);

    Task AddAsync(T entity, CancellationToken ct = default);

    void Update(T entity);

    void Remove(T entity);

    /// <summary>Persists all pending changes; returns the number of affected rows.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
