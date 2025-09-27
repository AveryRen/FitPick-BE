namespace FitPick_EXE201.Models.DTOs
{
    public class ReminderCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime? Scheduledat { get; set; }
        public bool? IsDone { get; set; }
    }

    public class ReminderResponseDto
    {
        public int Notificationid { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime? Scheduledat { get; set; }
        public bool? IsDone { get; set; }
        public bool? Isread { get; set; }
    }
}
