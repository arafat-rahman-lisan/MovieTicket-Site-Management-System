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

            [MaxLength(300)]
            public string? PosterUrl { get; set; }

            public MovieStatus Status { get; set; }
            public DateTime? ReleaseDate { get; set; }

            [Column(TypeName = "decimal(3,1)")]
            public decimal? ImdbRating { get; set; }
            public string? SmallPosterPath { get; set; }   // maps to sposter
            public string? BigPosterPath { get; set; }     // maps to bposter

        public MovieGenre Genre { get; set; }

            // RELATIONS
            // Movie (1) -> (N) Show
            public ICollection<Show> Shows { get; set; } = new List<Show>();
        }
}
