using System.Collections.Generic;
using System.Threading.Tasks;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Services.Interfaces
{
    public interface IShowNotesService
    {
        Task<IEnumerable<ShowNote>> GetAllAsync();
        Task<ShowNote?> GetByIdAsync(long id);
        Task AddAsync(ShowNote note);
        Task UpdateAsync(ShowNote note);
        Task DeleteAsync(long id);
    }
}
