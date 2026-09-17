namespace OrbitAOS.V6.Domain.Interfaces;

/// <summary>
/// Unit of Work interface for coordinating multiple repository operations within a single transaction.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>Commits all pending changes to the database asynchronously.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
