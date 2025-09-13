using Movie_Site_Management_System.Services.Interfaces;
using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.Data.Services.Interfaces;

namespace Movie_Site_Management_System.Services.Service
{
    public class BookingsService : IBookingsService
    {
        public Task AddAsync(Booking booking)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Booking>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Booking?> GetByIdAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Booking booking)
        {
            throw new NotImplementedException();
        }
    }
}
