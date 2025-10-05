using System.Collections.Generic;

namespace EVMManagement.API.DTOs.Response
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public int? ErrorCode { get; set; }

        public static ApiResponse<T> CreateSuccess(T data, string message = "")
        {
            return new ApiResponse<T> { Success = true, Data = data, Message = message };
        }

        public static ApiResponse<T> CreateFail(string message, List<string>? errors = null, int? errorCode = null)
        {
            if (errorCode == null)
            {
                errorCode = 400; 
            }
            return new ApiResponse<T> { Success = false, Message = message, Errors = errors ?? new List<string>(), ErrorCode = errorCode };
        }

        public static ApiResponse<T> CreateFail(Exception ex)
        {
            int errorCode = GetErrorCodeFromException(ex);
            return new ApiResponse<T> { Success = false, Message = ex.Message, Errors = new List<string> { ex.Message }, ErrorCode = errorCode };
        }

        private static int GetErrorCodeFromException(Exception ex)
        {
            return ex switch
            {
                ArgumentException or ArgumentNullException => 400,
                UnauthorizedAccessException => 401,
                KeyNotFoundException => 404,
                _ => 500
            };
        }
    }
}