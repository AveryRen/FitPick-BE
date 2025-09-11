using AutoMapper;
using FitPick_EXE201.Data;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using FitPick_EXE201.Repositories.Repo;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Services
{
    public class NotificationService
    {
        private readonly INotificationRepo _repo;
        private readonly INotificationTypeRepo _typeRepo;
        private readonly IMapper _mapper;
        private readonly FitPickContext _context;

        public NotificationService(INotificationRepo repo, INotificationTypeRepo typeRepo,IMapper mapper, FitPickContext context)
        {
            _context = context;
            _repo = repo;
            _typeRepo = typeRepo;
            _mapper = mapper;
        }

        /// <summary>
        /// Gửi 1 thông báo cho user
        /// </summary>
        public async Task<NotificationDTO> SendNotificationAsync(
            int userId, string title, string message, int? typeId = null, DateTime? scheduleAt = null)
        {
            // kiểm tra loại thông báo có tồn tại không
            if (typeId.HasValue)
            {
                var type = await _typeRepo.GetByIdAsync(typeId.Value);
                if (type == null)
                {
                    throw new KeyNotFoundException($"NotificationType với id {typeId} không tồn tại.");
                }
            }

            var notification = new Notification
            {
                Userid = userId,
                Title = title,
                Message = message,
                TypeId = typeId,
                Isread = false, // mặc định là chưa đọc
                Createdat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                Scheduledat = scheduleAt.HasValue
                        ? DateTime.SpecifyKind(scheduleAt.Value, DateTimeKind.Unspecified)
                        : null
            };

            var created = await _repo.CreateAsync(notification);
            return _mapper.Map<NotificationDTO>(created);
        }

        /// <summary>
        /// Lấy danh sách thông báo của 1 user
        /// </summary>
        public async Task<List<NotificationDTO>> GetNotificationsForUserAsync(int userId, bool? onlyUnread = null)
        {
            var query = _context.Notifications
                .Where(n => n.Userid == userId);

            if (onlyUnread.HasValue)
            {
                if (onlyUnread.Value)
                {
                    // chỉ lấy chưa đọc
                    query = query.Where(n => n.Isread == false || n.Isread == null);
                }
                else
                {
                    // chỉ lấy đã đọc
                    query = query.Where(n => n.Isread == true);
                }
            }

            var notifications = await query
                .OrderByDescending(n => n.Createdat)
                .ToListAsync();

            return _mapper.Map<List<NotificationDTO>>(notifications);
        }





        /// <summary>
        /// Đánh dấu thông báo là đã đọc
        /// </summary>
        public async Task<NotificationDTO> MarkAsReadAsync(int notificationId)
        {
            var notification = await _repo.GetByIdAsync(notificationId);
            if (notification == null)
            {
                throw new KeyNotFoundException($"Notification với id {notificationId} không tồn tại.");
            }

            notification.Isread = true;

            var success = await _repo.UpdateAsync(notificationId, notification);
            if (!success)
            {
                throw new Exception("Cập nhật trạng thái thông báo thất bại.");
            }

            // lấy lại notification đã update để map sang DTO
            var updated = await _repo.GetByIdAsync(notificationId);
            return _mapper.Map<NotificationDTO>(updated);
        }


        /// <summary>
        /// Xóa thông báo
        /// </summary>
        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            var notification = await _repo.GetByIdAsync(notificationId);
            if (notification == null)
            {
                throw new KeyNotFoundException($"Notification với id {notificationId} không tồn tại.");
            }

            return await _repo.Delete(notificationId);
        }

        /// <summary>
        /// Tạo loại thông báo mới
        /// </summary>
        public async Task<NotificationTypeDTO> CreateTypeAsync(string name)
        {
            // kiểm tra trùng tên
            var exists = await _context.NotificationTypes.AnyAsync(t => t.Name == name);
            if (exists)
            {
                throw new InvalidOperationException($"NotificationType với tên '{name}' đã tồn tại.");
            }

            var type = new NotificationType { Name = name };
            var created = await _typeRepo.CreateAsync(type);

            return _mapper.Map<NotificationTypeDTO>(created);
        }

        /// <summary>
        /// Lấy tất cả loại thông báo
        /// </summary>
        public async Task<List<NotificationTypeDTO>> GetAllTypesAsync()
        {
            var types = await _typeRepo.GetAllAsync();
            return _mapper.Map<List<NotificationTypeDTO>>(types);
        }

        /// <summary>
        /// Xóa loại thông báo
        /// </summary>
        public async Task<bool> DeleteTypeAsync(int typeId)
        {
            var type = await _typeRepo.GetByIdAsync(typeId);
            if (type == null)
            {
                throw new KeyNotFoundException($"NotificationType với id {typeId} không tồn tại.");
            }

            var hasNotifications = await _context.Notifications.AnyAsync(n => n.TypeId == typeId);
            if (hasNotifications)
            {
                throw new InvalidOperationException("Không thể xóa loại thông báo vì vẫn còn thông báo đang tham chiếu đến.");
            }

            return await _typeRepo.Delete(typeId);
        }
    }
}
