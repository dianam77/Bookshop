using Core.ServiceFile;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FileDisplayController : ControllerBase
{
    private readonly FileService _fileService;
    private const string AdminBookShopBaseUrl = "https://localhost:7244"; // آدرس پروژه اول

    public FileDisplayController(FileService fileService)
    {
        _fileService = fileService;
    }

    [HttpGet("display/{fileName}")]
    public async Task<IActionResult> DisplayFile(string fileName)
    {
        var fileUrl = $"{AdminBookShopBaseUrl}/api/FileDownload/{fileName}";
        try
        {
            var memoryStream = await _fileService.DownloadFileAsync(fileUrl);
            memoryStream.Position = 0;

            return File(memoryStream, "application/octet-stream", fileName);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return NotFound("File not found in source project.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

   

}
