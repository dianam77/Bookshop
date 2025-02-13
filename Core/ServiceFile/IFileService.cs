using Microsoft.AspNetCore.Http;

namespace Core.ServiceFile
{
    public interface IFileService
    {
        // Save a file to a specified folder
        Task<string> Save(IFormFile file, string folder);

        Task<MemoryStream> DownloadFileAsync(string fileUrl);
        Task<StaticFileUploadInfoDto> UploadStaticFile(IFormFile file, string path);

        // Delete a file and return the result
        Task<string> DeleteFile(string filePath);

        // Save a file to the content root directory
        string SaveContentRoot(IFormFile file, string folder);

        // Save finger data in the content root directory
        string SaveContentRootByte(FingerDataDto fingerData, string folder);

        // Delete a file and return a service result
        ServiceResult Delete(string fileName, string folder);

        // Delete a file in the content root and return a service result
        ServiceResult DeleteContentRoot(string fileName, string folder);

        // Get the source path of a file in the specified folder
        string GetSrc(string folder, string fileName);
    }
}
