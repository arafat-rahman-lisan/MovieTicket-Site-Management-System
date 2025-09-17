using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Movie_Site_Management_System.Models
{
    // Ensure (HallId, RowLabel, SeatNumber) is unique inside a hall
    [Index(nameof(HallId), nameof(RowLabel), nameof(SeatNumber), IsUnique = true)]
    public class Seat
    {
        public long SeatId { get; set; }

        [Required(ErrorMessage = "Hall is required.")]
        public long HallId { get; set; }

        [Required(ErrorMessage = "Row label is required.")]
        [MaxLength(4, ErrorMessage = "Row label cannot exceed 4 characters.")]
        public string RowLabel { get; set; } = default!;

        [Required(ErrorMessage = "Seat number is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seat number must be greater than zero.")]
        public int SeatNumber { get; set; }

        [Required(ErrorMessage = "Seat type is required.")]
        public short SeatTypeId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Position X must be zero or positive.")]
        public int? PosX { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Position Y must be zero or positive.")]
        public int? PosY { get; set; }

        [Required(ErrorMessage = "Disabled status must be set.")]
        public bool IsDisabled { get; set; }

        // Relations
        public Hall Hall { get; set; } = default!;
        public SeatType SeatType { get; set; } = default!;
        public ICollection<ShowSeat> ShowSeats { get; set; } = new List<ShowSeat>();
        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
        public ICollection<SeatBlock> SeatBlocks { get; set; } = new List<SeatBlock>();
    }
}
