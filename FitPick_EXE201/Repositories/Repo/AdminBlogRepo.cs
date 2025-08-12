using FitPick_EXE201.Data;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace FitPick_EXE201.Repositories.Repo
{
    public class AdminBlogRepo : IAdminBlogRepo
    {
        private readonly FitPickContext _context;

        public AdminBlogRepo(FitPickContext context)
        {
            _context = context;
        }

        #region Blog CRUD
        public async Task<PagedResult<Blogpost>> GetAllAsync(
             string? search = null,
             string? authorName = null,
             int? categoryId = null,
             DateTime? startDate = null,
             DateTime? endDate = null,
             string? sortBy = "createdat",
             bool sortDesc = true,
             int pageNumber = 1,
             int pageSize = 10)
        {
            var query = _context.Blogposts
                .Include(b => b.Author)
                .Include(b => b.BlogMedia)
                .Include(b => b.Category)
                .AsQueryable(); // Admin xem tất cả, không lọc Status

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b =>
                    b.Title.Contains(search) ||
                    b.Content.Contains(search));
            }

            if (!string.IsNullOrEmpty(authorName))
            {
                query = query.Where(b => b.Author.Fullname.Contains(authorName));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.Categoryid == categoryId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(b => b.Createdat >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(b => b.Createdat <= endDate.Value);
            }

            query = (sortBy?.ToLower()) switch
            {
                "updatedat" => sortDesc ? query.OrderByDescending(b => b.Updatedat) : query.OrderBy(b => b.Updatedat),
                _ => sortDesc ? query.OrderByDescending(b => b.Createdat) : query.OrderBy(b => b.Createdat),
            };

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Blogpost>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<Blogpost?> GetByIdAsync(int postId)
        {
            return await _context.Blogposts
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.BlogMedia)
                .FirstOrDefaultAsync(b => b.Postid == postId);
        }

        public async Task<Blogpost> CreateAsync(Blogpost post)
        {
            post.Createdat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            post.Updatedat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            _context.Blogposts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<bool> UpdateAsync(Blogpost post)
        {
            var existing = await _context.Blogposts.FindAsync(post.Postid);
            if (existing == null) return false;

            existing.Title = post.Title;
            existing.Content = post.Content;
            existing.Status = post.Status;
            existing.Categoryid = post.Categoryid;
            existing.Updatedat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            _context.Blogposts.Update(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int postId)
        {
            var existing = await _context.Blogposts
                .FirstOrDefaultAsync(b => b.Postid == postId);
            if (existing == null) return false;

            _context.Blogposts.Remove(existing); 
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Blog Category CRUD
        public async Task<IEnumerable<BlogCategory>> GetAllCategoriesAsync()
        {
            return await _context.BlogCategories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<BlogCategory?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.BlogCategories
                .FirstOrDefaultAsync(c => c.Categoryid == categoryId);
        }
        #endregion

        #region Blog Media CRUD
        public async Task<IEnumerable<BlogMedium>> GetMediaByBlogIdAsync(int blogId)
        {
            return await _context.BlogMedia
                .Where(m => m.BlogId == blogId)
                .OrderBy(m => m.OrderIndex)
                .ToListAsync();
        }

        public async Task<BlogMedium?> GetMediaByIdAsync(int mediaId)
        {
            return await _context.BlogMedia
                .FirstOrDefaultAsync(m => m.MediaId == mediaId);
        }

        public async Task<BlogMedium> AddMediaAsync(BlogMedium media)
        {
            _context.BlogMedia.Add(media);
            await _context.SaveChangesAsync();
            return media;
        }

        public async Task<bool> UpdateMediaAsync(BlogMedium media)
        {
            var existing = await _context.BlogMedia.FindAsync(media.MediaId);
            if (existing == null) return false;

            existing.MediaUrl = media.MediaUrl;
            existing.MediaType = media.MediaType;
            existing.OrderIndex = media.OrderIndex;

            _context.BlogMedia.Update(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMediaAsync(int mediaId)
        {
            var existing = await _context.BlogMedia.FindAsync(mediaId);
            if (existing == null) return false;

            _context.BlogMedia.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAllMediaByBlogIdAsync(int blogId)
        {
            var medias = await _context.BlogMedia
                .Where(m => m.BlogId == blogId)
                .ToListAsync();

            if (!medias.Any()) return false;

            _context.BlogMedia.RemoveRange(medias);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        public IQueryable<Blogpost> GetQueryable()
        {
            return _context.Blogposts.AsQueryable();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default)
        {
            return await _context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        }

        public async Task AddMediaRangeAsync(IEnumerable<BlogMedium> medias)
        {
            _context.BlogMedia.AddRange(medias);
            await _context.SaveChangesAsync();
        }
    }
}
