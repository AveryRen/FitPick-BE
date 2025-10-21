using System.ComponentModel.DataAnnotations;

namespace FitPick_EXE201.Models.Requests
{
    public class ChangeAvatarBase64Request
    {
        [Required]
        public string Base64Data { get; set; } = null!;
        
        public string? FileName { get; set; }
        
        public string? MimeType { get; set; }
    }
}
