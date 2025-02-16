namespace Core.ServiceFile
{
    public enum ResponseStatus
    {
        Success,
        NotFound,  
        ServerError
    }

    public class ServiceResult
    {
        public ResponseStatus Status { get; set; } 
        public string Message { get; set; }
        public object Data { get; set; } 

        public ServiceResult(ResponseStatus status, string? message = null, object? data = null)
        {
            Status = status;
            Message = message ?? "";
            Data = data ?? new object();

        }
    }
}
