using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class ReminderRepo : IReminderRepo
    {
        private readonly FitPickContext _context;
        public ReminderRepo(FitPickContext context)
        {
            _context = context;
        }

        public async Task<Notification> CreateAsync(Notification reminder)
        {
            _context.Notifications.Add(reminder);
            await _context.SaveChangesAsync();
            return reminder;
        }

        public Task<List<Notification>> GetByUserIdAsync(int userId)
        {
            return _context.Notifications
                .Where(n => n.Userid == userId && n.TypeId == 1) // 1 = Reminder
                .OrderByDescending(n => n.Scheduledat)
            .ToListAsync();
        }

        public Task<Notification?> GetByIdAsync(int id, int userId)
        {
            return _context.Notifications
                .FirstOrDefaultAsync(n => n.Notificationid == id
                                        && n.Userid == userId
                && n.TypeId == 1);
        }

        public async Task<bool> UpdateAsync(Notification reminder)
        {
            _context.Notifications.Update(reminder);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Notification reminder)
        {
            _context.Notifications.Remove(reminder);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
