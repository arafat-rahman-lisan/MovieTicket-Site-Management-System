using Movie_Site_Management_System.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movie_Site_Management_System.Models
{
    public class ShowSeat
    {
        // COMPOSITE KEY: ShowId + SeatId (configured in DbContext)
        public long ShowId { get; set; }
        public long SeatId { get; set; }

        public ShowSeatStatus Status { get; set; } = ShowSeatStatus.AVAILABLE;
        public DateTime? HoldExpiresAt { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PriceAtBooking { get; set; }

        // RELATIONS
        // ShowSeat (N) -> (1) Show
        public Show Show { get; set; } = default!;

        // ShowSeat (N) -> (1) Seat
        public Seat Seat { get; set; } = default!;

    }
}
