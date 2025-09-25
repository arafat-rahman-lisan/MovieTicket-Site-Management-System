namespace Movie_Site_Management_System.ViewModels.Schedule
{
    public class ScheduleHallDTO
    {
        public long HallId { get; set; }
        public string HallName { get; set; } = "";
        public List<ScheduleHallSlotDTO> HallSlots { get; set; } = new();
    }
}
