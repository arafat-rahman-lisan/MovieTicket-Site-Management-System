using System.Collections.Generic;
using System.Threading.Tasks;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Data.Services.Interfaces
{
    public interface IHallsService
    {
        Task<IEnumerable<Hall>> GetAllAsync();
        Task<Hall?> GetByIdAsync(long id);
        Task AddAsync(Hall hall);
        Task UpdateAsync(Hall hall);
        Task DeleteAsync(long id);
    }
}
