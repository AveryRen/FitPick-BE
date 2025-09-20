using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitPick_EXE201.Services;
using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.DTOs;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [Route("api/admin/blogs")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminBlogController : ControllerBase
    {
        private readonly AdminBlogService _blogService;
        private readonly CloudinaryService _cloudinaryService;

        public AdminBlogController(AdminBlogService blogService, CloudinaryService cloudinaryService)
        {
            _blogService = blogService ?? throw new ArgumentNullException(nameof(blogService));
            _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
        }

        /// <summary>📜 Lấy danh sách blog (phân trang + lọc + sắp xếp)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<BlogResponse>>), 200)]
        public async Task<ActionResult<ApiResponse<PagedResult<BlogResponse>>>> GetAll(
            [FromQuery] string? search,
            [FromQuery] int? categoryId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = "createdat",
            [FromQuery] bool sortDesc = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _blogService.GetQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(b => b.Title.Contains(search) || b.Content.Contains(search));
            if (categoryId.HasValue)
                query = query.Where(b => b.Categoryid == categoryId.Value);
            if (startDate.HasValue)
                query = query.Where(b => b.Createdat >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(b => b.Createdat <= endDate.Value);

            query = (sortBy?.ToLower()) switch
            {
                "updatedat" => sortDesc ? query.OrderByDescending(b => b.Updatedat) : query.OrderBy(b => b.Updatedat),
                _ => sortDesc ? query.OrderByDescending(b => b.Createdat) : query.OrderBy(b => b.Createdat)
            };

            var totalItems = await query.CountAsync();
            var blogs = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(b => b.BlogMedia)
                .Include(b => b.Author)
                .ToListAsync();

            var result = new PagedResult<BlogResponse>
            {
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = blogs.Select(ToDto).ToList()
            };

            return Ok(ApiResponse<PagedResult<BlogResponse>>.SuccessResponse(result, "Lấy danh sách blog thành công"));
        }

        /// <summary>🔎 Lấy chi tiết blog theo Id</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<BlogResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<ActionResult<ApiResponse<BlogResponse>>> GetById(int id)
        {
            var blog = await _blogService.GetByIdAsync(id);
            if (blog == null)
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Không tìm thấy blog" }, "Thất bại"));

            return Ok(ApiResponse<BlogResponse>.SuccessResponse(ToDto(blog), "Lấy blog thành công"));
        }

        /// <summary>➕ Tạo blog mới</summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        public async Task<ActionResult<ApiResponse<string>>> CreateBlog(
            [FromForm] string title,
            [FromForm] string content,
            [FromForm] int categoryId,
            [FromForm] List<IFormFile>? files)
        {
            var authorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var blog = new Blogpost
            {
                Title = title,
                Content = content,
                Categoryid = categoryId,
                Authorid = authorId,
                Status = true,
                Createdat = DateTime.UtcNow,
                Updatedat = DateTime.UtcNow
            };

            var created = await _blogService.CreateAsync(blog);

            if (files != null && files.Any())
            {
                var mediaList = new List<(string mediaUrl, string fileName)>();
                foreach (var file in files)
                {
                    var url = await _cloudinaryService.UploadFileAsync(file);
                    if (!string.IsNullOrEmpty(url))
                        mediaList.Add((url, file.FileName));
                }
                await _blogService.AddMediaRangeByFilesAsync(created.Postid, mediaList);
            }

            return Ok(ApiResponse<string>.SuccessResponse("OK", "Tạo blog thành công"));
        }

        /// <summary>✏️ Cập nhật blog</summary>
        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<ActionResult<ApiResponse<string>>> UpdateBlog(
            int id,
            [FromForm] string title,
            [FromForm] string content,
            [FromForm] int categoryId,
            [FromForm] bool status,
            [FromForm] List<IFormFile>? files)
        {
            var existing = await _blogService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Không tìm thấy blog" }, "Thất bại"));

            existing.Title = title;
            existing.Content = content;
            existing.Categoryid = categoryId;
            existing.Status = status;
            existing.Updatedat = DateTime.UtcNow;

            await _blogService.UpdateAsync(existing);

            await _blogService.DeleteAllMediaByBlogIdAsync(id);
            if (files != null && files.Any())
            {
                var mediaList = new List<(string mediaUrl, string fileName)>();
                foreach (var file in files)
                {
                    var url = await _cloudinaryService.UploadFileAsync(file);
                    if (!string.IsNullOrEmpty(url))
                        mediaList.Add((url, file.FileName));
                }
                await _blogService.AddMediaRangeByFilesAsync(id, mediaList);
            }

            return Ok(ApiResponse<string>.SuccessResponse("OK", "Cập nhật blog thành công"));
        }

        /// <summary>🗑️ Xóa blog</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<ActionResult<ApiResponse<string>>> DeleteBlog(int id)
        {
            var success = await _blogService.DeleteAsync(id);
            if (!success)
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Không tìm thấy blog" }, "Thất bại"));

            return Ok(ApiResponse<string>.SuccessResponse("OK", "Xóa blog thành công"));
        }

        #region Private Mapper
        private static BlogResponse ToDto(Blogpost b) => new()
        {
            Postid = b.Postid,
            Title = b.Title,
            Content = b.Content,
            Categoryid = b.Categoryid,
            Status = b.Status,
            Createdat = b.Createdat,
            Updatedat = b.Updatedat,
            Medias = b.BlogMedia?.Select(m => new BlogMediaResponse
            {
                MediaId = m.MediaId,
                MediaUrl = m.MediaUrl,
                MediaType = m.MediaType,
                OrderIndex = m.OrderIndex
            }).ToList(),
            Author = b.Author == null ? null : new AuthorResponse
            {
                UserId = b.Author.Userid,
                UserName = b.Author.Fullname
            }
        };
        #endregion
    }
}
