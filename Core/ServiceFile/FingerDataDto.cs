namespace Core.ServiceFile
{
    public class FingerDataDto
    {
        // Initialize properties with default values
        public byte[] ImageData { get; set; } = Array.Empty<byte>(); // Default empty byte array
        public string NewFileName { get; set; } = string.Empty; // Default empty string
        public string Name { get; set; } = string.Empty; // Default empty string
        public DateTime DateCaptured { get; set; } = DateTime.MinValue; // Default DateTime value

        // Optionally, you can also provide a constructor to allow initialization:
        public FingerDataDto(byte[] imageData, string newFileName, string name, DateTime dateCaptured)
        {
            ImageData = imageData;
            NewFileName = newFileName;
            Name = name;
            DateCaptured = dateCaptured;
        }
    }
}
