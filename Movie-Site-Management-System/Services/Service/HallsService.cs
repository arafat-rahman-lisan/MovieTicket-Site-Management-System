// File: Services/Service/HallsService.cs
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.BaseImpl;
using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.Services.Interfaces;

namespace Movie_Site_Management_System.Services.Service
{
    public class HallsService : EntityBaseRepository<Hall>, IHallsService
    {
        public HallsService(AppDbContext context) : base(context) { }
    }
}
