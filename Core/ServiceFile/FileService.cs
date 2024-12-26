using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Core.ServiceFile
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly RestClient _client;
        private readonly string _storagePath;
        private readonly HttpClient _httpClient;
        private readonly string _downloadFolder;
        private readonly IHttpClientFactory _httpClientFactory;
        // سازنده با تزریق HttpClient
        public FileService(IWebHostEnvironment environment, IConfiguration configuration, HttpClient httpClient, IHttpClientFactory httpClientFactory)
        {
            _environment = environment;
            _httpClient = httpClient; // تزریق HttpClient
            _httpClientFactory = httpClientFactory;
            _storagePath = Path.GetFullPath(Path.Combine(_environment.WebRootPath, "..", "File", "Uploads"));
            Console.WriteLine($"Resolved storage path: {_storagePath}");
            _client = new RestClient("https://localhost:7244");
            // تنظیم مسیر ذخیره فایل‌ها در پوشه wwwroot/Downloads
            _downloadFolder = Path.Combine(_environment.WebRootPath, "Downloads");
            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
        }


        public ServiceResult Delete(string fileName, string Folder)
        {
            try
            {
                var Path = $"{_environment.WebRootPath}\\File\\{Folder}\\{fileName}";
                if(File.Exists(Path))
                {
                    File.Delete(Path);
                    return new ServiceResult(ResponseStatus.Success, null);

                }
                else
                {
                    return new ServiceResult(ResponseStatus.NotFound, null);
                }

            }catch (Exception)
            {
                return new ServiceResult(ResponseStatus.ServerError, null);
            }
        }


        public string GetSrc(string Folder, string fileName)
        {
            try
            {
                var src = $"/File/{Folder}/{fileName}"; // Adjusted to be a URL
                return src;
            }
            catch (Exception)
            {
                throw; // Optionally, log the exception or handle it differently
            }
        }

        public async Task<string> Save(IFormFile file, string folder)
        {
            try
            {
                var extension = Path.GetExtension(file.FileName).TrimStart('.');
                var fileName = Guid.NewGuid() + "." + extension;

                // Create folder path based on the custom storage path
                var folderDirectory = Path.Combine(_storagePath, folder);
                var path = Path.Combine(folderDirectory, fileName);

                // Ensure the directory exists
                if (!Directory.Exists(folderDirectory))
                {
                    Directory.CreateDirectory(folderDirectory);
                }

                // Save the file to the custom path
                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    await file.CopyToAsync(fileStream);
                }

                Console.WriteLine($"File saved to: {path}"); // Debug logging
                return fileName; // Return filename for saving to database or referencing in views
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while saving the file: {ex.Message}");
                return null;
            }
        }


        public string SaveContentRoot(IFormFile file, string Folder)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(file.Name);
                var extes = file.FileName.Split('.').Last();
                var fileName = Guid.NewGuid() + "." + extes;
                var FolderDirectory = $"{_environment.ContentRootPath}\\{Folder}";
                var Paht = Path.Combine(FolderDirectory, fileName);
                var MemoryStream = new MemoryStream();
                file.OpenReadStream().CopyToAsync(MemoryStream);
                if (!Directory.Exists(FolderDirectory))
                {
                    Directory.CreateDirectory(FolderDirectory);
                }
                using (var filesave = new FileStream(Paht, FileMode.Create, FileAccess.Write))
                {
                    MemoryStream.WriteTo(filesave);
                }
                return fileName;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public ServiceResult DeleteContentRoot(string fileName, string Folder)
        {
            try
            {
                var Path = $"{_environment.ContentRootPath}\\{Folder}\\{fileName}";
                string Dpth = Path.Replace("/", "\\");
                string normalizedFilePath = Regex.Replace(Dpth, @"\\{2,}", @"\");
                if (File.Exists(normalizedFilePath))
                {
                    File.Delete(Path);
                    return new ServiceResult(ResponseStatus.Success, null);
                }
                else
                {
                    return new ServiceResult(ResponseStatus.NotFound, null);
                }
            }
            catch (Exception)
            {
                return new ServiceResult(ResponseStatus.ServerError, null);
            }
        }

        public string SaveContentRootByte(FingerDataDto fingerData, string folder)
        {
            if (fingerData == null)
            {
                throw new ArgumentNullException(nameof(fingerData), "fingerData cannot be null");
            }

            if (fingerData.ImageData == null || fingerData.NewFileName == null)
            {
                throw new ArgumentException("ImageData and NewFileName must be provided");
            }

            try
            {
                string fileName = $"{fingerData.NewFileName}.jpg";
                using (MemoryStream memoryStream = new MemoryStream(fingerData.ImageData))
                {
                    System.Drawing.Image image = System.Drawing.Image.FromStream(memoryStream);
                    var folderDirectory = $"{_environment.ContentRootPath}\\{folder}";
                    var path = Path.Combine(folderDirectory, fileName);
                    if (!Directory.Exists(folderDirectory))
                    {
                        Directory.CreateDirectory(folderDirectory);
                    }
                    image.Save(path);
                }
                return fileName;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                throw new Exception("An error occurred while saving the image", ex);
            }

        }
        public async Task<StaticFileUploadInfoDto> UploadStaticFile(IFormFile file, string path)
        {
            if (file is null) { return null; }

            var request = new RestRequest($"/UploadFile?path={path}", Method.Post);

            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                bytes = ms.ToArray();
            }
            request.AddFile(file.FileName, bytes, file.FileName, file.ContentType);


            var response = await _client.ExecutePostAsync<StaticFileUploadInfoDto>(request);
            //response.Data.StatusCode = response.StatusCode;

            return response.Data;
        }

        public async Task<string> DeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return "";
                }
                var request = new RestRequest($"/RemoveFile?path={filePath}", Method.Delete);
                var response = await _client.ExecuteAsync(request);

                return response.StatusCode == HttpStatusCode.OK ? "ok" : response.ErrorMessage;
            }
            catch (Exception err)
            {
                return err.InnerException.Message;
            }
        }

        public object Get_httpClient()
        {
            return _httpClient;
        }

        // متد دانلود و ذخیره فایل


        public async Task<MemoryStream> DownloadFileAsync(string imageFilePath)
        {
            string baseUrl = "https://localhost:7244";

            var fullImageUrl = new Uri(new Uri(baseUrl), imageFilePath); 

            var client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync(fullImageUrl);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                throw new FileNotFoundException("File not found at specified URL.");
            }

            var memoryStream = new MemoryStream();
            await response.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }



    }





}

