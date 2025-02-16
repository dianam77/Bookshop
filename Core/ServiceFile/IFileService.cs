using Microsoft.AspNetCore.Http;

namespace Core.ServiceFile
{
    public interface IFileService
    {
        Task<string> Save(IFormFile file, string folder);

        Task<MemoryStream> DownloadFileAsync(string fileUrl);
        Task<StaticFileUploadInfoDto> UploadStaticFile(IFormFile file, string path);

        Task<string> DeleteFile(string filePath);

        string SaveContentRoot(IFormFile file, string folder);

        string SaveContentRootByte(FingerDataDto fingerData, string folder);

        ServiceResult Delete(string fileName, string folder);

        ServiceResult DeleteContentRoot(string fileName, string folder);

        string GetSrc(string folder, string fileName);
    }
}
