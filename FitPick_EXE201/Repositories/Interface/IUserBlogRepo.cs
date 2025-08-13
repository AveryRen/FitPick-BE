using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IUserBlogRepo
    {
        // Blog CRUD
        Task<PagedResult<Blogpost>> GetAllAsync(
             string? search = null,
             string? authorName = null, 
             int? categoryId = null,
             DateTime? startDate = null,
             DateTime? endDate = null,
             string? sortBy = "createdat",
             bool sortDesc = true,
             int pageNumber = 1,
             int pageSize = 10);

        Task<Blogpost?> GetByIdAsync(int postId);
        Task<Blogpost> CreateAsync(Blogpost post);
        Task<bool> UpdateAsync(Blogpost post, int currentUserId, string currentUserRole);
        Task<bool> DeleteAsync(int postId, int currentUserId, string currentUserRole);
        Task<IEnumerable<BlogCategory>> GetAllCategoriesAsync();
        Task<BlogCategory?> GetCategoryByIdAsync(int categoryId);

        // Blog Media CRUD
        Task<IEnumerable<BlogMedium>> GetMediaByBlogIdAsync(int blogId);
        Task<BlogMedium?> GetMediaByIdAsync(int mediaId);
        Task<BlogMedium> AddMediaAsync(BlogMedium media);
        Task<bool> UpdateMediaAsync(BlogMedium media);
        Task<bool> DeleteMediaAsync(int mediaId);
        Task<bool> DeleteAllMediaByBlogIdAsync(int blogId);

        IQueryable<Blogpost> GetQueryable();
        Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default);
        Task AddMediaRangeAsync(IEnumerable<BlogMedium> medias);

    }
}
