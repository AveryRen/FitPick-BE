using Microsoft.AspNetCore.Mvc;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using FitPick_EXE201.Helpers;
using System.Security.Claims;
using FitPick_EXE201.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Controllers
{
    [Route("api/userblog")]
    [ApiController]
    public class UserBlogController : ControllerBase
    {
        private readonly UserBlogService _blogService;
        private readonly CloudinaryService _cloudinaryService;

        public UserBlogController(UserBlogService blogService, CloudinaryService cloudinaryService)
        {
            _blogService = blogService ?? throw new ArgumentNullException(nameof(blogService));
            _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(
              [FromQuery] string? search,
              [FromQuery] string? authorName,
              [FromQuery] int? categoryId,
              [FromQuery] DateTime? startDate,
              [FromQuery] DateTime? endDate,
              [FromQuery] string? sortBy = "createdat",
              [FromQuery] bool sortDesc = true,
              [FromQuery] int pageNumber = 1,
              [FromQuery] int pageSize = 10)
        {
            var blogsQuery = _blogService.GetQueryable();

            // Filter search
            if (!string.IsNullOrWhiteSpace(search))
            {
                blogsQuery = blogsQuery.Where(b =>
                    b.Title.Contains(search) ||
                    b.Content.Contains(search));
            }

            // Filter author by name (partial match)
            if (!string.IsNullOrWhiteSpace(authorName))
            {
                blogsQuery = blogsQuery.Where(b => b.Author != null && b.Author.Fullname.Contains(authorName));
            }

            // Filter category
            if (categoryId.HasValue)
            {
                blogsQuery = blogsQuery.Where(b => b.Categoryid == categoryId.Value);
            }

            // Filter date range
            if (startDate.HasValue)
            {
                blogsQuery = blogsQuery.Where(b => b.Createdat >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                blogsQuery = blogsQuery.Where(b => b.Createdat <= endDate.Value);
            }

            // Sorting
            blogsQuery = (sortBy?.ToLower()) switch
            {
                "updatedat" => sortDesc ? blogsQuery.OrderByDescending(b => b.Updatedat) : blogsQuery.OrderBy(b => b.Updatedat),
                _ => sortDesc ? blogsQuery.OrderByDescending(b => b.Createdat) : blogsQuery.OrderBy(b => b.Createdat),
            };

            // Paging
            var totalItems = await blogsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var blogs = await blogsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(b => b.BlogMedia)
                .Include(b => b.Author)
                .ToListAsync();

            // Map sang DTO
            var blogResponses = blogs.Select(blog => new BlogResponse
            {
                Postid = blog.Postid,
                Title = blog.Title,
                Content = blog.Content,
                Categoryid = blog.Categoryid,
                Status = blog.Status,
                Createdat = blog.Createdat,
                Updatedat = blog.Updatedat,
                Medias = blog.BlogMedia?.Select(m => new BlogMediaResponse
                {
                    MediaId = m.MediaId,
                    MediaUrl = m.MediaUrl,
                    MediaType = m.MediaType,
                    OrderIndex = m.OrderIndex
                }).ToList(),
                Author = blog.Author == null ? null : new AuthorResponse
                {
                    UserId = blog.Author.Userid,
                    UserName = blog.Author.Fullname
                }
            }).ToList();

            // Trả về dữ liệu kèm thông tin phân trang
            var result = new
            {
                TotalItems = totalItems,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = blogResponses
            };

            return Ok(ApiResponse<object>.SuccessResponse(result, "Lấy danh sách blog thành công"));
        }

        // Lấy blog theo ID
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var blogEntity = await _blogService.GetByIdAsync(id);
            if (blogEntity == null)
            {
                return NotFound(ApiResponse<BlogResponse>.ErrorResponse(
                    new List<string> { "Không tìm thấy blog" },
                    "Blog không tồn tại"
                ));
            }

            // Map Blogpost entity sang BlogResponse DTO
            var blogDto = new BlogResponse
            {
                Postid = blogEntity.Postid,
                Title = blogEntity.Title,
                Content = blogEntity.Content,
                Categoryid = blogEntity.Categoryid,
                Status = blogEntity.Status,
                Createdat = blogEntity.Createdat,
                Updatedat = blogEntity.Updatedat,
                Medias = blogEntity.BlogMedia?.Select(m => new BlogMediaResponse
                {
                    MediaId = m.MediaId,
                    MediaUrl = m.MediaUrl,
                    MediaType = m.MediaType,
                    OrderIndex = m.OrderIndex
                }).ToList(),
                Author = blogEntity.Author != null ? new AuthorResponse
                {
                    UserId = blogEntity.Author.Userid,
                    UserName = blogEntity.Author.Fullname
                } : null
            };

            return Ok(ApiResponse<BlogResponse>.SuccessResponse(blogDto, "Lấy blog thành công"));
        }

        // Helper method map từ Blogpost + BlogMedia sang BlogResponse
        private BlogResponse MapToBlogResponse(Blogpost blogpost, List<BlogMedium>? medias = null)
        {
            return new BlogResponse
            {
                Postid = blogpost.Postid,
                Title = blogpost.Title,
                Content = blogpost.Content,
                Categoryid = blogpost.Categoryid,
                Status = blogpost.Status,
                Createdat = blogpost.Createdat,
                Updatedat = blogpost.Updatedat,
                Medias = medias?.Select(m => new BlogMediaResponse
                {
                    MediaId = m.MediaId,
                    MediaUrl = m.MediaUrl,
                    MediaType = m.MediaType,
                    OrderIndex = m.OrderIndex
                }).ToList(),
                Author = blogpost.Author == null ? null : new AuthorResponse
                {
                    UserId = blogpost.Author.Userid,
                    UserName = blogpost.Author.Fullname
                }
            };
        }


        // Tạo blog mới
        [HttpPost]
        [Authorize(Roles = "Admin,Premium,User")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateBlogWithFiles(
            [FromForm] string title,
            [FromForm] string content,
            [FromForm] int categoryId,
            [FromForm] List<IFormFile>? files)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Không xác định được người dùng" },
                    "Tạo blog thất bại"
                ));
            }

            var mediaFiles = new List<(string mediaUrl, string fileName)>();
            if (files != null && files.Any())
            {
                foreach (var file in files)
                {
                    var url = await _cloudinaryService.UploadFileAsync(file);
                    if (!string.IsNullOrEmpty(url))
                    {
                        mediaFiles.Add((url, file.FileName));
                    }
                }
            }

            var blogPost = new Blogpost
            {
                Title = title,
                Content = content,
                Categoryid = categoryId,
                Authorid = userId,
                Createdat = DateTime.UtcNow,
                Updatedat = DateTime.UtcNow,
                Status = true
            };

            var created = await _blogService.CreateAsync(blogPost);

             if (mediaFiles.Any())
            {
                await _blogService.AddMediaRangeByFilesAsync(created.Postid, mediaFiles);
            }

            var medias = (await _blogService.GetMediaByBlogIdAsync(created.Postid)).ToList();
            var response = MapToBlogResponse(created, medias);

            return CreatedAtAction(nameof(GetById), new { id = created.Postid },
                ApiResponse<BlogResponse>.SuccessResponse(response, "Tạo blog thành công"));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Premium,User")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateBlogWithFiles(
    int id,
            [FromForm] string title,
            [FromForm] string content,
            [FromForm] int categoryId,
            [FromForm] bool? status,
            [FromForm] List<IFormFile>? files)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdClaim, out int userId) || string.IsNullOrEmpty(roleClaim))
            {
                return Unauthorized(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Không xác định được người dùng hoặc vai trò" },
                    "Cập nhật thất bại"
                ));
            }

            var existing = await _blogService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Không tìm thấy blog" },
                    "Cập nhật thất bại"
                ));
            }

            if (roleClaim != "Admin" && existing.Authorid != userId)
            {
                return Forbid();
            }

            using var transaction = await _blogService.BeginTransactionAsync();

            try
            {
                // Cập nhật blogpost
                existing.Title = title;
                existing.Content = content;
                existing.Categoryid = categoryId;
                existing.Status = status ?? existing.Status;
                existing.Updatedat = DateTime.Now; 
                var success = await _blogService.UpdateAsync(existing, userId, roleClaim);
                if (!success)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, ApiResponse<string>.ErrorResponse(
                        new List<string> { "Lỗi khi cập nhật blog" },
                        "Cập nhật thất bại"
                    ));
                }

                // Xóa media cũ
                await _blogService.DeleteAllMediaByBlogIdAsync(id);

                // Upload file media mới (nếu có)
                var mediaFiles = new List<(string mediaUrl, string fileName)>();
                if (files != null && files.Any())
                {
                    foreach (var file in files)
                    {
                        var url = await _cloudinaryService.UploadFileAsync(file);
                        if (!string.IsNullOrEmpty(url))
                        {
                            mediaFiles.Add((url, file.FileName));
                        }
                    }
                }

                if (mediaFiles.Any())
                {
                    await _blogService.AddMediaRangeByFilesAsync(id, mediaFiles);
                }

                await transaction.CommitAsync();

                var medias = (await _blogService.GetMediaByBlogIdAsync(id)).ToList();
                var response = MapToBlogResponse(existing, medias);

                return Ok(ApiResponse<BlogResponse>.SuccessResponse(response, "Cập nhật blog thành công"));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); 
                return StatusCode(500, ApiResponse<string>.ErrorResponse(
                    new List<string> { "Lỗi hệ thống khi cập nhật blog" },
                    "Cập nhật thất bại"
                ));
            }
        }


        // Xóa blog
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Premium,User")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdClaim, out int userId) || string.IsNullOrEmpty(roleClaim))
            {
                return Unauthorized(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Không xác định được người dùng hoặc vai trò" },
                    "Xóa thất bại"
                ));
            }

            var success = await _blogService.DeleteAsync(id, userId, roleClaim);

            if (!success)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Không tìm thấy blog hoặc bạn không có quyền xóa" },
                    "Xóa thất bại"
                ));
            }

            return Ok(ApiResponse<string>.SuccessResponse("OK", "Xóa blog thành công"));
        }
    }
}
