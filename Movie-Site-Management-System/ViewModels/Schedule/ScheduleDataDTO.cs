namespace Movie_Site_Management_System.ViewModels.Schedule
{
    public class ScheduleDataDTO
    {
        public List<ScheduleTheatreDTO> Theatres { get; set; } = new();
        public List<long> OccupiedHallSlotIds { get; set; } = new();
        public string Date { get; set; } = "";
    }
}
