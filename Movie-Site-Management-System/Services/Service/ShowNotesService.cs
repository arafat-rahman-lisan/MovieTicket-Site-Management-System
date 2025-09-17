// File: Services/Service/ShowNotesService.cs
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.BaseImpl;
using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.Services.Interfaces;

namespace Movie_Site_Management_System.Services.Service
{
    public class ShowNotesService : EntityBaseRepository<ShowNote>, IShowNotesService
    {
        public ShowNotesService(AppDbContext context) : base(context) { }
    }
}
