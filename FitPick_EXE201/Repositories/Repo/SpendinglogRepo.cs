using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Repositories.Repo
{
    public class SpendinglogRepo : BaseRepo<Spendinglog, int>, ISpendinglogRepo
    {
        private readonly FitPickContext _context;
        public SpendinglogRepo(FitPickContext context) : base(context)
        {
            _context = context;
        }
    }
}
