namespace FitPick_EXE201.Helpers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<String>? Error { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Error = null
            };
        }

        public static ApiResponse<T> ErrorResponse(List<string> errors, string errorMessage)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = errorMessage,
                Data = default,
                Error = errors
            };
        }
    }
}

