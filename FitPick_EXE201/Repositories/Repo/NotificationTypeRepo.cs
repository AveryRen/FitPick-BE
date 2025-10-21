using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class NotificationTypeRepo : BaseRepo<NotificationType, int>, INotificationTypeRepo
    {
        private readonly FitPickContext _context;
        public NotificationTypeRepo(FitPickContext context) : base(context)
        {
            _context = context;
        }

        public async Task<NotificationType?> GetByNameAsync(string name)
        {
            return await _context.NotificationTypes
                .FirstOrDefaultAsync(nt => nt.Name == name);
        }
    }
}
