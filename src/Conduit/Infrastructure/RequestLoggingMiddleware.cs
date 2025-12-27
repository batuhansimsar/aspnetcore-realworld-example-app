using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Conduit.Infrastructure;

/// <summary>
/// Middleware for logging HTTP requests with timing information.
/// Logs request method, path, status code, and elapsed time.
/// </summary>
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        
        logger.LogInformation(
            "Request started: {Method} {Path}",
            requestMethod,
            requestPath
        );

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (statusCode >= 500)
            {
                logger.LogError(
                    "Request completed: {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                    requestMethod,
                    requestPath,
                    statusCode,
                    elapsedMs
                );
            }
            else if (statusCode >= 400)
            {
                logger.LogWarning(
                    "Request completed: {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                    requestMethod,
                    requestPath,
                    statusCode,
                    elapsedMs
                );
            }
            else
            {
                logger.LogInformation(
                    "Request completed: {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                    requestMethod,
                    requestPath,
                    statusCode,
                    elapsedMs
                );
            }
        }
    }
}
