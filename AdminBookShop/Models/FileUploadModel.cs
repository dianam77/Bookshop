using Microsoft.AspNetCore.Http;

namespace AdminBookShop.Models
{
    public class FileUploadModel
    {
        public IFormFile File { get; set; }
    }
}
