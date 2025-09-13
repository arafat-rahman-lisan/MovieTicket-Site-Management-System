using   Movie_Site_Management_System.Data.Services.Interfaces;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Data.Services.Service
{
    public class HallsService : IHallsService
    {
        public Task AddAsync(Hall hall)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Hall>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Hall?> GetByIdAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Hall hall)
        {
            throw new NotImplementedException();
        }
    }
}
