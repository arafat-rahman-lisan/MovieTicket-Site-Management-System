using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.Services.Interfaces;

namespace Movie_Site_Management_System.Services.Service
{
    public class BookingSeatsService : IBookingSeatsService
    {
        public Task AddAsync(BookingSeat bookingSeat)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BookingSeat>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<BookingSeat?> GetByIdAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(BookingSeat bookingSeat)
        {
            throw new NotImplementedException();
        }
    }
}
