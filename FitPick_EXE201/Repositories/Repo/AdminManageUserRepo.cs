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

        public AdminManageUserRepo(FitPickContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync(
            int currentAdminId,
            string? searchKeyword,
            string? sortBy,
            bool sortDesc,
            int? genderId,
            int? roleId,
            bool? status
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
                    (u.Country != null && u.Country.ToLower().Contains(lowerKeyword)) ||
                    (u.City != null && u.City.ToLower().Contains(lowerKeyword))
                );
            }

            if (genderId.HasValue)
            {
                query = query.Where(u => u.GenderId == genderId.Value);
            }

            if (roleId.HasValue)
            {
                query = query.Where(u => u.RoleId == roleId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(u => u.Status == status.Value);
            }

            // Sort
            query = sortBy?.ToLower() switch
            {
                "fullname" => sortDesc ? query.OrderByDescending(u => u.Fullname) : query.OrderBy(u => u.Fullname),
                "email" => sortDesc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "age" => sortDesc ? query.OrderByDescending(u => u.Age) : query.OrderBy(u => u.Age),
                "country" => sortDesc ? query.OrderByDescending(u => u.Country) : query.OrderBy(u => u.Country),
                "city" => sortDesc ? query.OrderByDescending(u => u.City) : query.OrderBy(u => u.City),
                "createdat" => sortDesc ? query.OrderByDescending(u => u.Createdat) : query.OrderBy(u => u.Createdat),
                "updatedat" => sortDesc ? query.OrderByDescending(u => u.Updatedat) : query.OrderBy(u => u.Updatedat),
                _ => query.OrderBy(u => u.Userid) // default sort
            };

            return await query.ToListAsync();
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
                    City = u.City,
                    Role = u.Role != null ? u.Role.Name : "",
                    Status = u.Status, 
                    GenderId = (int)u.GenderId,
                    RoleId = u.RoleId
                })
                .FirstOrDefaultAsync();
        }



        // Tạo mới user
        public async Task<User> CreateUserAsync(User user)
        {
            if (user.RoleId <= 0)
            {
                user.RoleId = 2;
            }

            if (!string.IsNullOrWhiteSpace(user.Passwordhash))
            {
                user.Passwordhash = BCrypt.Net.BCrypt.HashPassword(user.Passwordhash);
            }
            user.Createdat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            user.Updatedat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }


        public async Task<bool> UpdateUserAsync(int id, AdminUserDetailDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            // Chỉ cập nhật những field cho phép
            if (!string.IsNullOrWhiteSpace(dto.Fullname))
                user.Fullname = dto.Fullname;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email;

            if (dto.GenderId > 0) // GenderId là int, internal set, >0 mới cập nhật
                user.GenderId = dto.GenderId;

            if (dto.Age.HasValue)
                user.Age = dto.Age;

            if (dto.Height.HasValue)
                user.Height = dto.Height;

            if (dto.Weight.HasValue)
                user.Weight = dto.Weight;

            if (!string.IsNullOrWhiteSpace(dto.Country))
                user.Country = dto.Country;

            if (!string.IsNullOrWhiteSpace(dto.City))
                user.City = dto.City;

            if (dto.RoleId.HasValue && dto.RoleId.Value > 0)
                user.RoleId = dto.RoleId;

            if (dto.Status.HasValue)
                user.Status = dto.Status;

            user.Updatedat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false; 
            user.Status = false; 
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
