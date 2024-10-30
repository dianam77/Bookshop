using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bookshop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly string _storagePath;

        public FileController(IConfiguration configuration)
        {
           
            var relativePath = configuration["FileUpload:StoragePath"];
            _storagePath = Path.Combine(AppContext.BaseDirectory, relativePath);
        }

        [HttpGet("GetFile")]
        public IActionResult DownloadFile(string fileName)
        {
            var fullPath = Path.Combine(_storagePath, fileName);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("File is not found");
            }

            var contentType = "application/octet-stream";
            return PhysicalFile(fullPath, contentType, fileName);
        }
    }
}
