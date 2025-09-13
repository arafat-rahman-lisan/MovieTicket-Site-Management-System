using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.Services.Interfaces;

namespace Movie_Site_Management_System.Data.Services.Service
{
    public class ShowSeatsService : IShowSeatsService
    {
        public Task AddAsync(ShowSeat showSeat)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(long showId, long seatId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ShowSeat>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ShowSeat?> GetByIdAsync(long showId, long seatId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(ShowSeat showSeat)
        {
            throw new NotImplementedException();
        }
    }
}
