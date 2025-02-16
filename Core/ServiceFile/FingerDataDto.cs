namespace Core.ServiceFile
{
    public class FingerDataDto
    {
        public byte[] ImageData { get; set; } = Array.Empty<byte>(); 
        public string NewFileName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime DateCaptured { get; set; } = DateTime.MinValue;

        public FingerDataDto(byte[] imageData, string newFileName, string name, DateTime dateCaptured)
        {
            ImageData = imageData;
            NewFileName = newFileName;
            Name = name;
            DateCaptured = dateCaptured;
        }
    }
}
