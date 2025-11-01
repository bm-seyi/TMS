using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace API.ExceptionHandlers
{
    public sealed class ExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ExceptionHandler> _logger;

        public ExceptionHandler(ILogger<ExceptionHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            httpContext.Response.ContentType = "application/problem+json";

            var (statusCode, problem) = exception switch
            {

                OperationCanceledException => (
                    StatusCodes.Status499ClientClosedRequest,
                    new ProblemDetails
                    {
                        Title = "Request Cancelled",
                        Detail = "The request was cancelled, possibly due to a timeout or client disconnect.",
                        Status = StatusCodes.Status499ClientClosedRequest,
                        Type = "https://developers.cloudflare.com/support/troubleshooting/http-status-codes/4xx-client-error/error-499/"
                    }
                ),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Title = "An unexpected error occurred.",
                        Detail = "Something went wrong while processing your request.",
                        Status = StatusCodes.Status500InternalServerError,
                        Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-500-internal-server-error"
                    }
                )
            };

            _logger.LogError(exception, "An error occurred while processing the request. Path: {Path}", httpContext.Request.Path.ToString().Replace("\r", "").Replace("\n", ""));

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

            return true;
        }
    }
}
