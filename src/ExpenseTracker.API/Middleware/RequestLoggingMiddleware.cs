using System.Diagnostics;

namespace ExpenseTracker.API.Middleware;

public class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            var userId = context.User
                             .FindFirst("sub")
                             ?.Value
                         ?? "anonymous";

            var ipAddress =
                context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown";

            logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms. UserId={UserId}, IP={IpAddress}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                userId,
                ipAddress);
        }
    }
}