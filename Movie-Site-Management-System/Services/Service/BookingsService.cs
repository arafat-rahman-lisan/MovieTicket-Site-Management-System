// File: Services/Service/BookingsService.cs
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.BaseImpl;
using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.Services.Interfaces;

namespace Movie_Site_Management_System.Services.Service
{
    public class BookingsService : EntityBaseRepository<Booking>, IBookingsService
    {
        public BookingsService(AppDbContext context) : base(context) { }
    }
}
