namespace Movie_Site_Management_System.ViewModels.Schedule
{
    public class ScheduleHallSlotDTO
    {
        public long HallSlotId { get; set; }
        public string Start { get; set; } = "--:--";
        public string End { get; set; } = "--:--";
        public bool IsOccupied { get; set; }
    }
}
