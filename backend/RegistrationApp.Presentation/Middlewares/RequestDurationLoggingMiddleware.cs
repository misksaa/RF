using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace RegistrationApp.Presentation.Middlewares;

public class RequestDurationLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestDurationLoggingMiddleware> _logger;

    public RequestDurationLoggingMiddleware(RequestDelegate next, ILogger<RequestDurationLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        var method = context.Request.Method;
        var path = context.Request.Path;
        var statusCode = context.Response.StatusCode;
        var elapsedMs = stopwatch.ElapsedMilliseconds;

        _logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
            method,
            path,
            statusCode,
            elapsedMs);
    }
}
