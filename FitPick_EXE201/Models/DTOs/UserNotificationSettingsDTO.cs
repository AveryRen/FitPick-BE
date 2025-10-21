namespace FitPick_EXE201.Models.DTOs
{
    public class UserNotificationSettingsDTO
    {
        public int UserId { get; set; }
        public bool NotificationsEnabled { get; set; }
    }

    public class UpdateNotificationSettingsRequest
    {
        public bool NotificationsEnabled { get; set; }
    }
}
