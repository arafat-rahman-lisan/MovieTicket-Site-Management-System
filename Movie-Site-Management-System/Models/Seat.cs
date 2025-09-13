using System.ComponentModel.DataAnnotations;

namespace Movie_Site_Management_System.Models
{
    public class Seat
    {
        public long SeatId { get; set; }

        // FK -> Hall
        [Required(ErrorMessage = "Hall is required.")]
        public long HallId { get; set; }

        [Required(ErrorMessage = "Row label is required.")]
        [MaxLength(4, ErrorMessage = "Row label cannot exceed 4 characters.")]
        public string RowLabel { get; set; } = default!;

        [Required(ErrorMessage = "Seat number is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seat number must be greater than zero.")]
        public int SeatNumber { get; set; }

        // FK -> SeatType
        [Required(ErrorMessage = "Seat type is required.")]
        public short SeatTypeId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Position X must be zero or positive.")]
        public int? PosX { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Position Y must be zero or positive.")]
        public int? PosY { get; set; }

        [Required(ErrorMessage = "Disabled status must be set.")]
        public bool IsDisabled { get; set; }

        // RELATIONS
        // Seat (N) -> (1) Hall
        public Hall Hall { get; set; } = default!;

        // Seat (N) -> (1) SeatType
        public SeatType SeatType { get; set; } = default!;

        // Seat (1) -> (N) ShowSeat
        public ICollection<ShowSeat> ShowSeats { get; set; } = new List<ShowSeat>();

        // Seat (1) -> (N) BookingSeat
        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();

        // Seat (1) -> (N) SeatBlock
        public ICollection<SeatBlock> SeatBlocks { get; set; } = new List<SeatBlock>();
    }
}
