using System.Collections.Generic;
using System.Threading.Tasks;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Services.Interfaces
{
    public interface ISeatsService
    {
        Task<IEnumerable<Seat>> GetAllAsync();
        Task<Seat?> GetByIdAsync(long id);
        Task AddAsync(Seat seat);
        Task UpdateAsync(Seat seat);
        Task DeleteAsync(long id);
    }
}
