using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class ReminderService
    {
        private readonly IReminderRepo _repo;
        public ReminderService(IReminderRepo repo)
        {
            _repo = repo;
        }

        // Tạo reminder mới
        public async Task<ReminderResponseDto> CreateAsync(int userId, ReminderCreateDto dto)
        {
            var reminder = new Notification
            {
                Userid = userId,
                Title = dto.Title,
                Message = dto.Message,
                TypeId = 1, // Reminder
                Scheduledat = dto.Scheduledat.HasValue
                    ? DateTime.SpecifyKind(dto.Scheduledat.Value, DateTimeKind.Unspecified)
                    : null,
                IsDone = dto.IsDone ?? false,
                Isread = false,
                Createdat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
            };

            var created = await _repo.CreateAsync(reminder);

            // Trả về DTO response
            return new ReminderResponseDto
            {
                Notificationid = created.Notificationid,
                Title = created.Title,
                Message = created.Message,
                Scheduledat = created.Scheduledat,
                IsDone = created.IsDone,
                Isread = created.Isread
            };
        }

        // Lấy danh sách reminders của user
        public async Task<List<ReminderResponseDto>> GetByUserIdAsync(int userId)
        {
            var list = await _repo.GetByUserIdAsync(userId);
            return list.Select(n => new ReminderResponseDto
            {
                Notificationid = n.Notificationid,
                Title = n.Title,
                Message = n.Message,
                Scheduledat = n.Scheduledat,
                IsDone = n.IsDone,
                Isread = n.Isread
            }).ToList();
        }

        // Cập nhật reminder
        public async Task<bool> UpdateAsync(int id, int userId, ReminderCreateDto dto)
        {
            var existing = await _repo.GetByIdAsync(id, userId);
            if (existing == null) return false;

            existing.Title = dto.Title;
            existing.Message = dto.Message;
            existing.Scheduledat = dto.Scheduledat.HasValue
                ? DateTime.SpecifyKind(dto.Scheduledat.Value, DateTimeKind.Unspecified)
                : null;
            existing.IsDone = dto.IsDone ?? existing.IsDone;

            return await _repo.UpdateAsync(existing);
        }

        // Xóa reminder
        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var existing = await _repo.GetByIdAsync(id, userId);
            if (existing == null) return false;
            return await _repo.DeleteAsync(existing);
        }
    }
}
