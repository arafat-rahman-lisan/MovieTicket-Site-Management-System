// File: Data/Base/IEntityBaseRepository.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Site_Management_System.Data.Base
{
    /// <summary>
    /// Generic async repository contract for EF entities.
    /// Uses params object[] for keys so it works with different PK types/names.
    /// </summary>
    public interface IEntityBaseRepository<T> where T : class
    {
        /// <summary>Returns a queryable for advanced scenarios (Include/Where in services/controllers).</summary>
        IQueryable<T> Query();

        /// <summary>Get all entities (no-tracking).</summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>Get one entity by its key value(s).</summary>
        Task<T?> GetByIdAsync(params object[] keyValues);

        /// <summary>Add a new entity and save.</summary>
        Task AddAsync(T entity);

        /// <summary>Update an entity and save.</summary>
        Task UpdateAsync(T entity);

        /// <summary>Delete by key and save.</summary>
        Task DeleteAsync(params object[] keyValues);
    }
}
