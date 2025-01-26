namespace Core.ServiceFile
{
    // Define the ResponseStatus enum
    public enum ResponseStatus
    {
        Success,
        NotFound,  // Correct spelling
        ServerError
    }

    public class ServiceResult
    {
        public ResponseStatus Status { get; set; } // Use ResponseStatus instead of bool
        public string Message { get; set; }
        public object Data { get; set; } // Optional, depending on your needs

        // Constructor accepting ResponseStatus
        public ServiceResult(ResponseStatus status, string message = null, object data = null)
        {
            Status = status;
            Message = message;
            Data = data;
        }
    }
}
