using Movie_Site_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Services.Interfaces;
using Movie_Site_Management_System.Data;

namespace Movie_Site_Management_System.Services.Service
{
    public class TheatresService : ITheatresService
    {
        private readonly AppDbContext _context;

        public TheatresService(AppDbContext context)
        {
            _context = context;
        }

        // Synchronous Add (your request)
        public void Add(Theatre theatre)
        {
            _context.Theatres.Add(theatre);
            _context.SaveChanges();
        }

        // Async Add
        public async Task AddAsync(Theatre theatre)
        {
            await _context.Theatres.AddAsync(theatre);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Theatre>> GetAllAsync()
        {
            return await _context.Theatres
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<Theatre?> GetByIdAsync(long id)
        {
            return await _context.Theatres
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TheatreId == id);
        }

        public async Task UpdateAsync(Theatre theatre)
        {
            _context.Theatres.Update(theatre);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var theatre = await _context.Theatres.FindAsync(id);
            if (theatre != null)
            {
                _context.Theatres.Remove(theatre);
                await _context.SaveChangesAsync();
            }
        }
    }
}
