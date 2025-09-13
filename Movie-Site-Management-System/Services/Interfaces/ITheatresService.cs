using System.Collections.Generic;
using System.Threading.Tasks;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Services.Interfaces
{
    public interface ITheatresService
    {
        Task<IEnumerable<Theatre>> GetAllAsync();
        Task<Theatre?> GetByIdAsync(long id);
        Task AddAsync(Theatre theatre);
        Task UpdateAsync(Theatre theatre);
        Task DeleteAsync(long id);
    }
}
