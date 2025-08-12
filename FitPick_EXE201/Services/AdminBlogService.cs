using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace FitPick_EXE201.Services
{
    public class AdminBlogService
    {
        private readonly IAdminBlogRepo _adminBlogRepo;

        public AdminBlogService(IAdminBlogRepo adminBlogRepo)
        {
            _adminBlogRepo = adminBlogRepo;
        }

        #region Blogpost
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
            return await _adminBlogRepo.GetAllAsync(search, authorName, categoryId, startDate, endDate, sortBy, sortDesc, pageNumber, pageSize);
        }

        public async Task<Blogpost?> GetByIdAsync(int postId)
        {
            return await _adminBlogRepo.GetByIdAsync(postId);
        }

        public async Task<Blogpost> CreateAsync(Blogpost post)
        {
            return await _adminBlogRepo.CreateAsync(post);
        }

        public async Task<bool> UpdateAsync(Blogpost post)
        {
            return await _adminBlogRepo.UpdateAsync(post);
        }

        public async Task<bool> DeleteAsync(int postId)
        {
            return await _adminBlogRepo.DeleteAsync(postId);
        }
        #endregion

        #region BlogCategory
        public async Task<IEnumerable<BlogCategory>> GetAllCategoriesAsync()
        {
            return await _adminBlogRepo.GetAllCategoriesAsync();
        }

        public async Task<BlogCategory?> GetCategoryByIdAsync(int categoryId)
        {
            return await _adminBlogRepo.GetCategoryByIdAsync(categoryId);
        }
        #endregion

        #region BlogMedia
        public async Task<IEnumerable<BlogMedium>> GetMediaByBlogIdAsync(int blogId)
        {
            return await _adminBlogRepo.GetMediaByBlogIdAsync(blogId);
        }

        public async Task<BlogMedium?> GetMediaByIdAsync(int mediaId)
        {
            return await _adminBlogRepo.GetMediaByIdAsync(mediaId);
        }

        public async Task<BlogMedium> AddMediaAsync(BlogMedium media)
        {
            return await _adminBlogRepo.AddMediaAsync(media);
        }

        public async Task<bool> UpdateMediaAsync(BlogMedium media)
        {
            return await _adminBlogRepo.UpdateMediaAsync(media);
        }

        public async Task<bool> DeleteMediaAsync(int mediaId)
        {
            return await _adminBlogRepo.DeleteMediaAsync(mediaId);
        }

        public async Task<bool> DeleteAllMediaByBlogIdAsync(int blogId)
        {
            return await _adminBlogRepo.DeleteAllMediaByBlogIdAsync(blogId);
        }
        #endregion

        #region Helpers
        public IQueryable<Blogpost> GetQueryable()
        {
            return _adminBlogRepo.GetQueryable();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default)
        {
            return await _adminBlogRepo.BeginTransactionAsync(isolationLevel, cancellationToken);
        }

        public async Task AddMediaRangeAsync(IEnumerable<BlogMedium> medias)
        {
            await _adminBlogRepo.AddMediaRangeAsync(medias);
        }

        // Thêm media từ danh sách file (mediaUrl + fileName)
        public async Task AddMediaRangeByFilesAsync(int blogId, IEnumerable<(string mediaUrl, string fileName)> files)
        {
            var medias = files.Select((f, index) => new BlogMedium
            {
                BlogId = blogId,
                MediaUrl = f.mediaUrl,
                MediaType = Path.GetExtension(f.fileName).ToLower().Contains("mp4") ? "video" : "image",
                OrderIndex = index
            }).ToList();

            await _adminBlogRepo.AddMediaRangeAsync(medias);
        }


        // Xác định loại media từ file extension
        public string GetMediaType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".png" => "image",
                ".jpg" => "image",
                ".jpeg" => "image",
                ".gif" => "gif",
                ".mp4" => "video",
                ".mp3" => "audio",
                ".pdf" => "pdf",
                _ => "other",
            };
        }
        #endregion
    }
}