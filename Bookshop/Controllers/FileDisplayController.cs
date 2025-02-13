using Core.ServiceFile;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FileDisplayController : ControllerBase
{
    private readonly IFileService _fileService;
    private const string AdminBookShopBaseUrl = "https://localhost:7244";

    public FileDisplayController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpGet("display/{fileName}")]
    public async Task<IActionResult> DisplayFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return BadRequest("File name cannot be empty.");
        }

        // Validate the file name for illegal characters
        if (fileName.Contains("<") || fileName.Contains(">") || fileName.Contains(":") || fileName.Contains("/") || fileName.Contains("\\"))
        {
            return BadRequest("Invalid file name.");
        }

        try
        {
            var fileStream = await _fileService.DownloadFileAsync(fileName);
            if (fileStream == null)
            {
                return NotFound("File not found in source project.");
            }

            return new FileStreamResult(fileStream, "application/octet-stream")
            {
                FileDownloadName = fileName
            };
        }
        catch (FileNotFoundException)
        {
            return NotFound("File not found in source project.");
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

}
