using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data.Enums;

namespace Movie_Site_Management_System.Models
{
    // Ensure one seat appears at most once per show
    [Index(nameof(ShowId), nameof(SeatId), IsUnique = true)]
    public class ShowSeat
    {
        public long ShowSeatId { get; set; }

        // FKs
        [Required]
        public long ShowId { get; set; }

        [Required]
        public long SeatId { get; set; }

        // Snapshot fields (copied at show creation time)
        [Required]
        public short SeatTypeId { get; set; }   // from Seat.SeatTypeId at snapshot

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 9999999999.99)]
        public decimal Price { get; set; }      // from SeatType.BasePrice at snapshot

        [Required]
        public ShowSeatStatus Status { get; set; } = ShowSeatStatus.Available;

        // ⏲️ When a seat is temporarily reserved. UTC; null when not held.
        [Column(TypeName = "datetime2")]
        public DateTime? HoldUntil { get; set; }

        // Concurrency token to avoid double-booking races
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // Navigations
        public Show? Show { get; set; }
        public Seat? Seat { get; set; }
        public SeatType? SeatType { get; set; }

        // Back-reference for BookingSeat (many BookingSeats can point to this ShowSeat)
        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
    }
}
