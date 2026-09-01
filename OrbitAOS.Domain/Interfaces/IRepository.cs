using OrbitAOS.Domain.Common;

namespace OrbitAOS.Domain.Interfaces
{
    /// <summary>
    /// Generic repository interface defining standard CRUD operations for domain entities.
    /// </summary>
    /// <typeparam name="T">The entity type, must derive from BaseEntity.</typeparam>
    public interface IRepository<T> where T : BaseEntity
    {
        /// <summary>Retrieves an entity by its unique identifier.</summary>
        Task<T?> GetByIdAsync(int id);

        /// <summary>Retrieves all entities of the given type.</summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>Adds a new entity to the repository.</summary>
        Task<T> AddAsync(T entity);

        /// <summary>Updates an existing entity in the repository.</summary>
        Task UpdateAsync(T entity);

        /// <summary>Deletes an entity from the repository by its identifier.</summary>
        Task DeleteAsync(int id);
    }
}
