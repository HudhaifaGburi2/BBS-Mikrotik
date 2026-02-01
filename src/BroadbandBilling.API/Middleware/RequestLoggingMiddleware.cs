namespace BroadbandBilling.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        
        _logger.LogInformation("Handling {Method} request for {Path}", requestMethod, requestPath);

        var startTime = DateTime.UtcNow;

        await _next(context);

        var duration = DateTime.UtcNow - startTime;
        var statusCode = context.Response.StatusCode;

        _logger.LogInformation(
            "Completed {Method} request for {Path} with status {StatusCode} in {Duration}ms",
            requestMethod,
            requestPath,
            statusCode,
            duration.TotalMilliseconds);
    }
}
