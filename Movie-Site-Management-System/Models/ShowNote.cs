namespace Movie_Site_Management_System.Models
{
    public class ShowNote
    {
        public long ShowNoteId { get; set; }

        // FK -> Show
        public long ShowId { get; set; }

        public string Note { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // RELATIONS
        // ShowNote (N) -> (1) Show
        public Show Show { get; set; } = default!;
    }
}
