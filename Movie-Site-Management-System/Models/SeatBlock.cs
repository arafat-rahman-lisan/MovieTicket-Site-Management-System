using System.ComponentModel.DataAnnotations;

namespace Movie_Site_Management_System.Models
{
    public class SeatBlock
    {
        public long SeatBlockId { get; set; }

        // FK -> Seat
        public long SeatId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [MaxLength(120)]
        public string? Reason { get; set; }

        // RELATIONS
        // SeatBlock (N) -> (1) Seat
        public Seat Seat { get; set; } = default!;
    }
}
