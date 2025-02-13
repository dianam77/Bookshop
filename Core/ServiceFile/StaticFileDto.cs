namespace Core.ServiceFile
{
    public class StaticFileUploadInfoDto
    {
        public string FileAddress { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;
        public int Duration { get; set; }
    }
}
