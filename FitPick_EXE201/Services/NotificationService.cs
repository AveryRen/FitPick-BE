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
        /// G?i 1 th�ng b�o cho user
        /// </summary>
        public async Task<NotificationDTO> SendNotificationAsync(
            int userId, string title, string message, int? typeId = null, DateTime? scheduleAt = null)
        {
            // ki?m tra lo?i th�ng b�o c� t?n t?i kh�ng
            if (typeId.HasValue)
            {
                var type = await _typeRepo.GetByIdAsync(typeId.Value);
                if (type == null)
                {
                    throw new KeyNotFoundException($"NotificationType v?i id {typeId} kh�ng t?n t?i.");
                }
            }

            var notification = new Notification
            {
                Userid = userId,
                Title = title,
                Message = message,
                TypeId = typeId,
                Isread = false, // m?c d?nh l� chua d?c
                Createdat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                Scheduledat = scheduleAt.HasValue
                        ? DateTime.SpecifyKind(scheduleAt.Value, DateTimeKind.Unspecified)
                        : null
            };

            var created = await _repo.CreateAsync(notification);
            return _mapper.Map<NotificationDTO>(created);
        }

        /// <summary>
        /// L?y danh s�ch th�ng b�o c?a 1 user
        /// </summary>
        public async Task<List<NotificationDTO>> GetNotificationsForUserAsync(int userId, bool? onlyUnread = null)
        {
            var query = _context.Notifications
                .Include(n => n.Type)
                .Where(n => n.Userid == userId);

            if (onlyUnread.HasValue)
            {
                if (onlyUnread.Value)
                {
                    // ch? l?y chua d?c
                    query = query.Where(n => n.Isread == false || n.Isread == null);
                }
                else
                {
                    // ch? l?y d� d?c
                    query = query.Where(n => n.Isread == true);
                }
            }

            var notifications = await query
                .OrderByDescending(n => n.Createdat)
                .ToListAsync();

            return _mapper.Map<List<NotificationDTO>>(notifications);
        }





        /// <summary>
        /// ��nh d?u th�ng b�o l� d� d?c
        /// </summary>
        public async Task<NotificationDTO> MarkAsReadAsync(int notificationId)
        {
            var notification = await _repo.GetByIdAsync(notificationId);
            if (notification == null)
            {
                throw new KeyNotFoundException($"Notification v?i id {notificationId} kh�ng t?n t?i.");
            }

            notification.Isread = true;

            var success = await _repo.UpdateAsync(notificationId, notification);
            if (!success)
            {
                throw new Exception("C?p nh?t tr?ng th�i th�ng b�o th?t b?i.");
            }

            // l?y l?i notification d� update d? map sang DTO
            var updated = await _repo.GetByIdAsync(notificationId);
            return _mapper.Map<NotificationDTO>(updated);
        }


        /// <summary>
        /// X�a th�ng b�o
        /// </summary>
        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            var notification = await _repo.GetByIdAsync(notificationId);
            if (notification == null)
            {
                throw new KeyNotFoundException($"Notification v?i id {notificationId} kh�ng t?n t?i.");
            }

            return await _repo.Delete(notificationId);
        }

        /// <summary>
        /// T?o lo?i th�ng b�o m?i
        /// </summary>
        public async Task<NotificationTypeDTO> CreateTypeAsync(string name)
        {
            // ki?m tra tr�ng t�n
            var exists = await _context.NotificationTypes.AnyAsync(t => t.Name == name);
            if (exists)
            {
                throw new InvalidOperationException($"NotificationType v?i t�n '{name}' d� t?n t?i.");
            }

            var type = new NotificationType { Name = name };
            var created = await _typeRepo.CreateAsync(type);

            return _mapper.Map<NotificationTypeDTO>(created);
        }

        /// <summary>
        /// L?y t?t c? lo?i th�ng b�o
        /// </summary>
        public async Task<List<NotificationTypeDTO>> GetAllTypesAsync()
        {
            var types = await _typeRepo.GetAllAsync();
            return _mapper.Map<List<NotificationTypeDTO>>(types);
        }

        /// <summary>
        /// X�a lo?i th�ng b�o
        /// </summary>
        public async Task<bool> DeleteTypeAsync(int typeId)
        {
            var type = await _typeRepo.GetByIdAsync(typeId);
            if (type == null)
            {
                throw new KeyNotFoundException($"NotificationType v?i id {typeId} kh�ng t?n t?i.");
            }

            var hasNotifications = await _context.Notifications.AnyAsync(n => n.TypeId == typeId);
            if (hasNotifications)
            {
                throw new InvalidOperationException("Kh�ng th? x�a lo?i th�ng b�o v� v?n c�n th�ng b�o dang tham chi?u d?n.");
            }

            return await _typeRepo.Delete(typeId);
        }

        /// <summary>
        /// Đánh dấu thông báo là đã đọc (với userId check)
        /// </summary>
        public async Task<NotificationDTO?> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _repo.GetByIdAsync(notificationId);
            if (notification == null || notification.Userid != userId)
            {
                return null; // Không tìm thấy hoặc không thuộc về user này
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
        /// Xóa thông báo (với userId check)
        /// </summary>
        public async Task<bool> DeleteNotificationAsync(int notificationId, int userId)
        {
            var notification = await _repo.GetByIdAsync(notificationId);
            if (notification == null || notification.Userid != userId)
            {
                return false; // Không tìm thấy hoặc không thuộc về user này
            }

            return await _repo.Delete(notificationId);
        }

        /// <summary>
        /// Lấy tất cả loại thông báo (alias)
        /// </summary>
        public async Task<List<NotificationTypeDTO>> GetAllNotificationTypesAsync()
        {
            return await GetAllTypesAsync();
        }
    }
}
