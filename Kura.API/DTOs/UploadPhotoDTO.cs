using System.ComponentModel.DataAnnotations;

namespace Kura.API.DTOs
{
    public class UploadPhotoDTO
    {
        [Required(ErrorMessage = "Image is required!")]
        public string Base64Image { get; set; } = string.Empty;
    }
}