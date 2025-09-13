using System.Collections.Generic;
using System.Threading.Tasks;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Services.Interfaces
{
    public interface IShowSeatsService
    {
        Task<IEnumerable<ShowSeat>> GetAllAsync();
        Task<ShowSeat?> GetByIdAsync(long showId, long seatId);
        Task AddAsync(ShowSeat showSeat);
        Task UpdateAsync(ShowSeat showSeat);
        Task DeleteAsync(long showId, long seatId);
    }
}
