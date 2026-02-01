namespace BroadbandBilling.Application.Common.DTOs;

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? StackTrace { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ErrorResponse()
    {
    }

    public ErrorResponse(int statusCode, string message, List<string>? errors = null)
    {
        StatusCode = statusCode;
        Message = message;
        Errors = errors ?? new List<string>();
    }

    public static ErrorResponse Create(int statusCode, string message, string? error = null)
    {
        var response = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = message
        };

        if (!string.IsNullOrWhiteSpace(error))
        {
            response.Errors.Add(error);
        }

        return response;
    }
}
