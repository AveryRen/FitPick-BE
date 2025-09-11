using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Repositories.Repo
{
    public class NotificationRepo : BaseRepo<Notification, int>, INotificationRepo
    {
        private readonly FitPickContext _context;
        public NotificationRepo(FitPickContext context) : base(context)
        {
            _context = context;
        }
    }
}
