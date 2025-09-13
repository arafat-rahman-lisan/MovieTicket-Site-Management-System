using System.Collections.Generic;
using System.Threading.Tasks;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Data.Services.Interfaces
{
    public interface ISeatBlocksService
    {
        Task<IEnumerable<SeatBlock>> GetAllAsync();
        Task<SeatBlock?> GetByIdAsync(long id);
        Task AddAsync(SeatBlock seatBlock);
        Task UpdateAsync(SeatBlock seatBlock);
        Task DeleteAsync(long id);
    }
}
