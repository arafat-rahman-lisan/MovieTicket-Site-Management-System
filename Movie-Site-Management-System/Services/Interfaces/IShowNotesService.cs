// File: Services/Interfaces/IShowNotesService.cs
using Movie_Site_Management_System.Data.Base;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Services.Interfaces
{
    public interface IShowNotesService : IEntityBaseRepository<ShowNote> { }
}
