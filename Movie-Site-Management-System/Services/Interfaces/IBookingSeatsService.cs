using System.Collections.Generic;
using System.Threading.Tasks;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Services.Interfaces
{
    public interface IBookingSeatsService
    {
        Task<IEnumerable<BookingSeat>> GetAllAsync();
        Task<BookingSeat?> GetByIdAsync(long id);
        Task AddAsync(BookingSeat bookingSeat);
        Task UpdateAsync(BookingSeat bookingSeat);
        Task DeleteAsync(long id);
    }
}
