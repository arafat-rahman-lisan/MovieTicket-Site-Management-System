// File: Data/Base/EntityBaseRepository.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Movie_Site_Management_System.Data.Base;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Site_Management_System.Data.BaseImpl
{
    /// <summary>
    /// Generic EF Core repository implementation.
    /// - No tracking for GetAll
    /// - Uses DbSet.FindAsync for PKs regardless of name/type
    /// - Update uses Entry(entity).State = Modified
    /// </summary>
    public class EntityBaseRepository<T> : IEntityBaseRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _set;

        public EntityBaseRepository(AppDbContext context)
        {
            _context = context;
            _set = _context.Set<T>();
        }

        public IQueryable<T> Query() => _set.AsQueryable();

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _set.AsNoTracking().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(params object[] keyValues)
        {
            // Works for single or composite keys.
            return await _set.FindAsync(keyValues);
        }

        public async Task AddAsync(T entity)
        {
            await _set.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            EntityEntry entry = _context.Entry(entity);
            entry.State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(params object[] keyValues)
        {
            var existing = await GetByIdAsync(keyValues);
            if (existing is null) return;

            _set.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }
}
