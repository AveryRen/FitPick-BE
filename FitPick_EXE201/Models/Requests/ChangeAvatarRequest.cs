using System.ComponentModel.DataAnnotations;

namespace FitPick_EXE201.Models.Requests
{
    public class ChangeAvatarRequest
    {
        [Required]
        public IFormFile Avatar { get; set; } = null!;
    }
}
