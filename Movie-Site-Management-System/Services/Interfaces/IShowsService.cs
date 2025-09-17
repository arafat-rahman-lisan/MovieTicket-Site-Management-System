// File: Services/Interfaces/IShowsService.cs
using Movie_Site_Management_System.Data.Base;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Services.Interfaces
{
    public interface IShowsService : IEntityBaseRepository<Show> { }
}
