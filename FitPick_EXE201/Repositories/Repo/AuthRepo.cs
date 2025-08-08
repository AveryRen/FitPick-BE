using FitPick_EXE201.Models.Entities;
using System.Security.Principal;
using FitPick_EXE201.Models;
using Microsoft.EntityFrameworkCore;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Repositories.Repo
{
    public class AuthRepo : IAuthRepo
    {
        private readonly FitPickDbContext _context;

        public AuthRepo(FitPickDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User account)
        {
            _context.Users.Add(account);
            await _context.SaveChangesAsync();
        }
        public async Task<User?> GetAccountByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Gender)
                .FirstOrDefaultAsync(u => u.Email == email);
        } 
        public async Task<User?> GetAccountByCredentialsAsync(string email, string password)
        {
            return await _context.Users
                .FirstOrDefaultAsync(a => a.Email == email && a.Passwordhash == password);
        } 
        public async Task UpdateAsync(User account)
        {
            _context.Users.Update(account);
            await _context.SaveChangesAsync();
        }
    }
}