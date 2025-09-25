namespace Movie_Site_Management_System.ViewModels.Schedule
{
    public class ScheduleTheatreDTO
    {
        public long TheatreId { get; set; }
        public string TheatreName { get; set; } = "";
        public List<ScheduleHallDTO> Halls { get; set; } = new();
    }
}
