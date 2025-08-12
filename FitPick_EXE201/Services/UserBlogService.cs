using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace FitPick_EXE201.Services
{
    public class UserBlogService
    {
        private readonly IUserBlogRepo _blogRepo;

        public UserBlogService(IUserBlogRepo blogRepo)
        {
            _blogRepo = blogRepo;
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
            return await _blogRepo.GetAllAsync(search, authorName, categoryId, startDate, endDate, sortBy, sortDesc, pageNumber, pageSize);
        }
         
        public async Task<Blogpost?> GetByIdAsync(int postId)
        {
            return await _blogRepo.GetByIdAsync(postId);
        }

        public async Task<Blogpost> CreateAsync(Blogpost post)
        {
            return await _blogRepo.CreateAsync(post);
        }

        public async Task<bool> UpdateAsync(Blogpost post, int currentUserId, string currentUserRole)
        {
            return await _blogRepo.UpdateAsync(post, currentUserId, currentUserRole);
        }

        public async Task<bool> DeleteAsync(int postId, int currentUserId, string currentUserRole)
        {
            return await _blogRepo.DeleteAsync(postId, currentUserId, currentUserRole);
        }
        #endregion

        #region BlogCategory
        public async Task<IEnumerable<BlogCategory>> GetAllCategoriesAsync()
        {
            return await _blogRepo.GetAllCategoriesAsync();
        }

        public async Task<BlogCategory?> GetCategoryByIdAsync(int categoryId)
        {
            return await _blogRepo.GetCategoryByIdAsync(categoryId);
        }
        #endregion

        #region BlogMedia
        public async Task<IEnumerable<BlogMedium>> GetMediaByBlogIdAsync(int blogId)
        {
            return await _blogRepo.GetMediaByBlogIdAsync(blogId);
        }

        public async Task<BlogMedium?> GetMediaByIdAsync(int mediaId)
        {
            return await _blogRepo.GetMediaByIdAsync(mediaId);
        }

        public async Task<BlogMedium> AddMediaAsync(BlogMedium media)
        {
            return await _blogRepo.AddMediaAsync(media);
        }

        public async Task<bool> UpdateMediaAsync(BlogMedium media)
        {
            return await _blogRepo.UpdateMediaAsync(media);
        }

        public async Task<bool> DeleteMediaAsync(int mediaId)
        {
            return await _blogRepo.DeleteMediaAsync(mediaId);
        }

        public async Task<bool> DeleteAllMediaByBlogIdAsync(int blogId)
        {
            return await _blogRepo.DeleteAllMediaByBlogIdAsync(blogId);
        }
        #endregion

        public IQueryable<Blogpost> GetQueryable()
        {
            return _blogRepo.GetQueryable();
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync(
                IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                CancellationToken cancellationToken = default)
        {
            return await _blogRepo.BeginTransactionAsync(isolationLevel, cancellationToken);
        }
        public async Task AddMediaRangeByFilesAsync(int blogId, IEnumerable<(string mediaUrl, string fileName)> files)
        {
            var medias = files.Select((f, index) => new BlogMedium
            {
                BlogId = blogId,
                MediaUrl = f.mediaUrl,
                MediaType = GetMediaType(f.fileName),
                OrderIndex = index
            }).ToList();

            await _blogRepo.AddMediaRangeAsync(medias);
        }
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
    }
} 
