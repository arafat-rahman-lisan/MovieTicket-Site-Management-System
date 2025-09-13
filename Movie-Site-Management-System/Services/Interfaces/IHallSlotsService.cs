using System.Collections.Generic;
using System.Threading.Tasks;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Services.Interfaces
{
    public interface IHallSlotsService
    {
        Task<IEnumerable<HallSlot>> GetAllAsync();
        Task<HallSlot?> GetByIdAsync(long id);
        Task AddAsync(HallSlot hallSlot);
        Task UpdateAsync(HallSlot hallSlot);
        Task DeleteAsync(long id);
    }
}
