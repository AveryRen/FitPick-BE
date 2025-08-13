using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitPick_EXE201.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly CloudinaryService _cloudinaryService;

        public UploadController(CloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("Không có file để upload.");

            var uploadResults = new List<string>();

            foreach (var file in files)
            {
                var url = await _cloudinaryService.UploadFileAsync(file);
                if (url != null)
                    uploadResults.Add(url);
            }

            return Ok(new { UploadedUrls = uploadResults });
        }
    }
}