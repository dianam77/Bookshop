using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.FileUpload
{
    public class FileUploadService : IFileUploadService
    {
        private readonly string _storagePath;

        public FileUploadService(IConfiguration configuration)
        {
            var relativePath = configuration["FileUpload:StoragePath"];
            _storagePath = Path.Combine(AppContext.BaseDirectory, relativePath);
            Console.WriteLine($"Storage path: {_storagePath}"); // Debugging output
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            try
            {
                if (!Directory.Exists(_storagePath))
                {
                    Directory.CreateDirectory(_storagePath);
                }

                if (file == null || file.Length == 0)
                {
                    throw new ArgumentNullException("File is empty or not provided");
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var fullPath = Path.Combine(_storagePath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return fileName;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error uploading file: {ex.Message}");
                throw;
            }
        }
    }
}