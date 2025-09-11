using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Repositories.Repo
{
    public class NotificationTypeRepo : BaseRepo<NotificationType, int>, INotificationTypeRepo
    {
        private readonly FitPickContext _context;
        public NotificationTypeRepo(FitPickContext context) : base(context)
        {
            _context = context;
        }
    }
}
