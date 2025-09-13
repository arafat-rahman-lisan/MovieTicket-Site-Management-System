using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.Services.Interfaces;

namespace Movie_Site_Management_System.Data.Services.Service
{
    public class ShowNotesService : IShowNotesService
    {
        public Task AddAsync(ShowNote note)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ShowNote>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ShowNote?> GetByIdAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(ShowNote note)
        {
            throw new NotImplementedException();
        }
    }
}
