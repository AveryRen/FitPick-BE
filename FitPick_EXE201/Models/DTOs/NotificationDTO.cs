namespace FitPick_EXE201.Models.DTOs
{
    public class NotificationDTO
    {
        public int NotificationId { get; set; }
        public int? UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public int? TypeId { get; set; }
        public string? TypeName { get; set; }  // lấy từ NotificationType
        public bool? IsRead { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ScheduledAt { get; set; }
    }

    public class NotificationTypeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
