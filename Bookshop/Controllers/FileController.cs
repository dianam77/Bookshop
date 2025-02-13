using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Core.FileUpload;

[Route("api/[controller]")]
[ApiController]
public class FileController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly IFileUploadService _fileUploadService;

    public FileController(IWebHostEnvironment env, IFileUploadService fileUploadService)
    {
        _env = env;
        _fileUploadService = fileUploadService;
    }

    [HttpGet("GetFile")]
    public IActionResult GetFile(string filename)
    {
        filename = filename.TrimStart('/');

        var filePath = Path.Combine(_env.WebRootPath, filename);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        var contentType = "image/jpeg"; // Adjust based on your file type

        return File(fileBytes, contentType);
    }

    [HttpPost("UploadFile")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty or not provided");
        }

        var fileName = await _fileUploadService.UploadFileAsync(file);
        return Ok(new { FileName = fileName });
    }
}
