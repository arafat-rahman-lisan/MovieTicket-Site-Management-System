using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.Services.Interfaces;

namespace Movie_Site_Management_System.Data.Services.Service
{
    public class SeatsService : ISeatsService
    {
        public Task AddAsync(Seat seat)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Seat>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Seat?> GetByIdAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Seat seat)
        {
            throw new NotImplementedException();
        }
    }
}
