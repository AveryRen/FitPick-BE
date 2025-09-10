using FitPick_EXE201.Data; // Namespace chứa FitPickContext
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class AdminManageUserRepo : IAdminManageUserRepo
    {
        private readonly FitPickContext _context;
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public AdminManageUserRepo(FitPickContext context)
        {
            _context = context;
        }
        public async Task<PagedResult<User>> GetAllUsersAsync(
             int currentAdminId,
             string? searchKeyword,
             string? sortBy,
             bool sortDesc,
             int? genderId,
             int? roleId,
             bool? status,
             int pageNumber = 1,
             int pageSize = 10
         )
        {
            var query = _context.Users
                .Where(u => u.Userid != currentAdminId)
                .Include(u => u.Role)
                .Include(u => u.Gender)
                .AsNoTracking()
                .AsQueryable();

            // Filter search keyword
            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                string lowerKeyword = searchKeyword.ToLower();
                query = query.Where(u =>
                    (u.Fullname != null && u.Fullname.ToLower().Contains(lowerKeyword)) ||
                    u.Email.ToLower().Contains(lowerKeyword) ||
                    (u.Country != null && u.Country.ToLower().Contains(lowerKeyword))
                );
            }

            if (genderId.HasValue)
                query = query.Where(u => u.GenderId == genderId.Value);

            if (roleId.HasValue)
                query = query.Where(u => u.RoleId == roleId.Value);

            if (status.HasValue)
                query = query.Where(u => u.Status == status.Value);

            // Sort
            query = sortBy?.ToLower() switch
            {
                "fullname" => sortDesc ? query.OrderByDescending(u => u.Fullname) : query.OrderBy(u => u.Fullname),
                "email" => sortDesc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "age" => sortDesc ? query.OrderByDescending(u => u.Age) : query.OrderBy(u => u.Age),
                "country" => sortDesc ? query.OrderByDescending(u => u.Country) : query.OrderBy(u => u.Country),
                "createdat" => sortDesc ? query.OrderByDescending(u => u.Createdat) : query.OrderBy(u => u.Createdat),
                "updatedat" => sortDesc ? query.OrderByDescending(u => u.Updatedat) : query.OrderBy(u => u.Updatedat),
                _ => query.OrderBy(u => u.Userid)
            };

            // Tổng số bản ghi trước phân trang
            var totalItems = await query.CountAsync();

            // Phân trang
            var skip = (pageNumber - 1) * pageSize;
            var items = await query.Skip(skip).Take(pageSize).ToListAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PagedResult<User>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                PageSize = pageSize,
                PageNumber = pageNumber
            };
        }


        // Lấy user theo ID
        public async Task<AdminUserDetailDto?> GetUserByIdForAdminAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Gender)
                .Where(u => u.Userid == id)
                .Select(u => new AdminUserDetailDto
                {
                    Fullname = u.Fullname,
                    Email = u.Email,
                    Gender = u.Gender != null ? u.Gender.Name : null,
                    Age = u.Age,
                    Height = u.Height,
                    Weight = u.Weight,
                    Country = u.Country,
                    Role = u.Role != null ? u.Role.Name : null,
                    Status = u.Status,
                    GenderId = (int)u.GenderId,
                    RoleId = u.RoleId,
                    AvatarUrl = u.AvatarUrl
                })
                .FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserEntityByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }
        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }


        public async Task<bool> UpdateUserAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.Userid);
            if (existingUser == null)
                return false;

            // Cập nhật field
            existingUser.Fullname = user.Fullname;
            existingUser.Email = user.Email;
            existingUser.GenderId = user.GenderId;
            existingUser.Age = user.Age;
            existingUser.Height = user.Height;
            existingUser.Weight = user.Weight;
            existingUser.Country = user.Country;
            existingUser.RoleId = user.RoleId;
            existingUser.Status = user.Status;

            existingUser.Updatedat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();

            return true;
        }



        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);  
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string newPasswordHash)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Passwordhash = newPasswordHash;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
