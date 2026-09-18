using Microsoft.EntityFrameworkCore;
using OrbitAOS.Domain.Common;
using OrbitAOS.Domain.Interfaces;
using OrbitAOS.Infrastructure.Data;

namespace OrbitAOS.Infrastructure.Repositories;

/// <summary>
/// Generic EF Core repository implementation providing standard CRUD operations.
/// </summary>
/// <typeparam name="T">The entity type that inherits from <see cref="BaseEntity"/>.</typeparam>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    /// <summary>The EF Core database context.</summary>
    protected readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="Repository{T}"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public Repository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.Set<T>().FindAsync(new object[] { id }, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Set<T>().ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is not null)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
