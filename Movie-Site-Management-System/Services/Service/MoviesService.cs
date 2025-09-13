using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.Services.Interfaces;

namespace Movie_Site_Management_System.Data.Services.Service
{
    public class MoviesService : IMoviesService
    {
        public Task AddAsync(Movie movie)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Movie>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Movie?> GetByIdAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Movie movie)
        {
            throw new NotImplementedException();
        }
    }
}
