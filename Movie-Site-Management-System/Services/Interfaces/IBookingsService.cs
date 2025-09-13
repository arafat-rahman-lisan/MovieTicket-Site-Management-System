using System.Collections.Generic;
using System.Threading.Tasks;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Data.Services.Interfaces
{
    public interface IBookingsService
    {
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(long id);
        Task AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
        Task DeleteAsync(long id);
    }
}
