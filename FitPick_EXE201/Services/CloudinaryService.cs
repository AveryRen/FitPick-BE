using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FitPick_EXE201.Settings;
using Microsoft.Extensions.Options;
using Npgsql.BackendMessages;
using System.Security.Principal;

namespace FitPick_EXE201.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        public CloudinaryService(IOptions<CloudinarySettings> options)
        {
            var settings = options.Value;
            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                // Bạn có thể thêm folder, transformation ở đây nếu muốn
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return uploadResult.SecureUrl.ToString(); // URL của file đã upload
            }

            return null;
        }
    }
}