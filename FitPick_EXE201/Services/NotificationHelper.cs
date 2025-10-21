using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Services
{
    public class NotificationHelper
    {
        private readonly FitPickContext _context;
        private readonly INotificationRepo _notificationRepo;
        private readonly INotificationTypeRepo _notificationTypeRepo;

        public NotificationHelper(
            FitPickContext context, 
            INotificationRepo notificationRepo, 
            INotificationTypeRepo notificationTypeRepo)
        {
            _context = context;
            _notificationRepo = notificationRepo;
            _notificationTypeRepo = notificationTypeRepo;
        }

        /// <summary>
        /// Tạo thông báo khi user thêm meal vào favorites
        /// </summary>
        public async Task CreateFavoriteNotificationAsync(int userId, string mealName)
        {
            await CreateNotificationAsync(userId, "Món ăn yêu thích", 
                $"Bạn đã thêm '{mealName}' vào danh sách yêu thích", "favorite");
        }

        /// <summary>
        /// Tạo thông báo khi user tạo meal plan mới
        /// </summary>
        public async Task CreateMealPlanNotificationAsync(int userId, DateTime date)
        {
            await CreateNotificationAsync(userId, "Thực đơn mới", 
                $"Thực đơn cho ngày {date:dd/MM/yyyy} đã được tạo thành công", "meal_plan");
        }

        /// <summary>
        /// Tạo thông báo nhắc nhở ăn uống
        /// </summary>
        public async Task CreateMealReminderNotificationAsync(int userId, string mealTime)
        {
            await CreateNotificationAsync(userId, "Nhắc nhở ăn uống", 
                $"Đã đến giờ {mealTime}! Hãy thưởng thức bữa ăn của bạn", "reminder");
        }

        /// <summary>
        /// Tạo thông báo khi có meal mới được đề xuất
        /// </summary>
        public async Task CreateMealSuggestionNotificationAsync(int userId, string mealName)
        {
            await CreateNotificationAsync(userId, "Đề xuất món ăn mới", 
                $"Chúng tôi có một món ăn mới phù hợp với bạn: '{mealName}'", "suggestion");
        }

        /// <summary>
        /// Tạo thông báo hệ thống
        /// </summary>
        public async Task CreateSystemNotificationAsync(int userId, string title, string message)
        {
            await CreateNotificationAsync(userId, title, message, "system");
        }

        /// <summary>
        /// Tạo thông báo khuyến mãi
        /// </summary>
        public async Task CreatePromotionNotificationAsync(int userId, string title, string message)
        {
            await CreateNotificationAsync(userId, title, message, "promotion");
        }

        /// <summary>
        /// Tạo thông báo cho tất cả users
        /// </summary>
        public async Task CreateBroadcastNotificationAsync(string title, string message, string typeName = "system")
        {
            var users = await _context.Users
                .Where(u => u.Status == true && u.NotificationsEnabled == true)
                .ToListAsync();
            
            foreach (var user in users)
            {
                await CreateNotificationAsync(user.Userid, title, message, typeName);
            }
        }

        /// <summary>
        /// Tạo thông báo cơ bản
        /// </summary>
        private async Task CreateNotificationAsync(int userId, string title, string message, string typeName)
        {
            try
            {
                // Kiểm tra xem user có cho phép nhận thông báo không
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Userid == userId);
                if (user == null || user.NotificationsEnabled == false)
                {
                    Console.WriteLine($"User {userId} has notifications disabled or user not found. Skipping notification.");
                    return;
                }

                // Tìm hoặc tạo notification type
                var notificationType = await _notificationTypeRepo.GetByNameAsync(typeName);
                if (notificationType == null)
                {
                    notificationType = new NotificationType { Name = typeName };
                    await _notificationTypeRepo.CreateAsync(notificationType);
                }

                // Tạo notification
                var notification = new Notification
                {
                    Userid = userId,
                    Title = title,
                    Message = message,
                    TypeId = notificationType.Id,
                    Isread = false,
                    Createdat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
                };

                await _notificationRepo.CreateAsync(notification);
            }
            catch (Exception ex)
            {
                // Log error nhưng không throw để không ảnh hưởng đến flow chính
                Console.WriteLine($"Error creating notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo thông báo có lịch trình
        /// </summary>
        public async Task CreateScheduledNotificationAsync(int userId, string title, string message, DateTime scheduleAt, string typeName = "reminder")
        {
            try
            {
                // Kiểm tra xem user có cho phép nhận thông báo không
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Userid == userId);
                if (user == null || user.NotificationsEnabled == false)
                {
                    Console.WriteLine($"User {userId} has notifications disabled or user not found. Skipping scheduled notification.");
                    return;
                }

                var notificationType = await _notificationTypeRepo.GetByNameAsync(typeName);
                if (notificationType == null)
                {
                    notificationType = new NotificationType { Name = typeName };
                    await _notificationTypeRepo.CreateAsync(notificationType);
                }

                var notification = new Notification
                {
                    Userid = userId,
                    Title = title,
                    Message = message,
                    TypeId = notificationType.Id,
                    Isread = false,
                    Createdat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                    Scheduledat = DateTime.SpecifyKind(scheduleAt, DateTimeKind.Unspecified)
                };

                await _notificationRepo.CreateAsync(notification);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating scheduled notification: {ex.Message}");
            }
        }
    }
}
