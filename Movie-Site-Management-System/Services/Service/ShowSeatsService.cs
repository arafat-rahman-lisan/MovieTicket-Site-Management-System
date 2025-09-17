// File: Services/Service/ShowSeatsService.cs
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.BaseImpl;
using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.Services.Interfaces;

namespace Movie_Site_Management_System.Services.Service
{
    public class ShowSeatsService : EntityBaseRepository<ShowSeat>, IShowSeatsService
    {
        public ShowSeatsService(AppDbContext context) : base(context) { }
    }
}
