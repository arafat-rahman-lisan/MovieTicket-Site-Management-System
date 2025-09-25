using Movie_Site_Management_System.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movie_Site_Management_System.Models
{
    public class Movie
    {
        public long MovieId { get; set; }

        [Required, MaxLength(160)]
        public string Title { get; set; } = default!;

        public int RuntimeMinutes { get; set; }

        [MaxLength(20)]
        public string? RatingCertificate { get; set; }

        public string? Synopsis { get; set; }

        // Optional legacy single poster field (you already also have Small/Big)
        [MaxLength(300)]
        public string? PosterUrl { get; set; }

        // ✅ enums aligned
        public MovieGenre Genre { get; set; } = MovieGenre.Unknown;
        public MovieStatus Status { get; set; } = MovieStatus.Unknown;

        public DateTime? ReleaseDate { get; set; }

        [Column(TypeName = "decimal(3,1)")]
        public decimal? ImdbRating { get; set; }

        public string? SmallPosterPath { get; set; }   // maps to sposter
        public string? BigPosterPath { get; set; }     // maps to bposter

        // RELATIONS
        public ICollection<Show> Shows { get; set; } = new List<Show>();
    }
}
