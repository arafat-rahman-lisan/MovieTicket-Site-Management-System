using System.Collections.Generic;
using System.Threading.Tasks;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Data.Services.Interfaces
{
    public interface ISeatTypesService
    {
        Task<IEnumerable<SeatType>> GetAllAsync();
        Task<SeatType?> GetByIdAsync(long id);
        Task AddAsync(SeatType seatType);
        Task UpdateAsync(SeatType seatType);
        Task DeleteAsync(long id);
    }
}
