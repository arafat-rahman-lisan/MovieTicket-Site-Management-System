using Movie_Site_Management_System.Data.Services.Interfaces;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Services.Service
{
    public class ShowsService : IShowsService
    {
        public Task AddAsync(Show show)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Show>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Show?> GetByIdAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Show show)
        {
            throw new NotImplementedException();
        }
    }
}
