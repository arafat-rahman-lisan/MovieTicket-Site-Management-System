using System.Collections.Generic;
using System.Threading.Tasks;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Services.Interfaces
{
    public interface IMoviesService
    {
        Task<IEnumerable<Movie>> GetAllAsync();
        Task<Movie?> GetByIdAsync(long id);
        Task AddAsync(Movie movie);
        Task UpdateAsync(Movie movie);
        Task DeleteAsync(long id);
    }
}
