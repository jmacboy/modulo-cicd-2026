using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.WebApi.ExceptionHandling
{
    /// <summary>
    /// Logs unhandled exceptions and returns a generic RFC 7807 response that never exposes
    /// exception details (type, message, stack trace) to the client, in any environment.
    /// </summary>
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private const string ProblemJsonContentType = "application/problem+json";

        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Unhandled exception for {Method} {Path} (traceId {TraceId})",
                httpContext.Request.Method, httpContext.Request.Path, httpContext.TraceIdentifier);

            var problem = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                Title = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError
            };
            problem.Extensions["traceId"] = httpContext.TraceIdentifier;

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(problem, options: null, contentType: ProblemJsonContentType, cancellationToken: cancellationToken);
            return true;
        }
    }
}