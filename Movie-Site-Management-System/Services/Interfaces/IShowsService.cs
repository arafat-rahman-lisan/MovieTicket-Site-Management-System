using System.Collections.Generic;
using System.Threading.Tasks;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Data.Services.Interfaces
{
    public interface IShowsService
    {
        Task<IEnumerable<Show>> GetAllAsync();
        Task<Show?> GetByIdAsync(long id);
        Task AddAsync(Show show);
        Task UpdateAsync(Show show);
        Task DeleteAsync(long id);
    }
}
