using System.Diagnostics;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using TMS.Core.Extensions;

namespace TMS.API.ExceptionHandlers
{
    public sealed class ExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ExceptionHandler> _logger;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.API.ExceptionHandlers.ExceptionHandler");
 
        public ExceptionHandler(ILogger<ExceptionHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
 
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            using Activity? activity = _activitySource.StartActivity("ExceptionHandler.TryHandleAsync");
 
            ProblemDetails problem = new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Detail = "Something went wrong while processing your request.",
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-500-internal-server-error"
            };
 
 
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = MediaTypeNames.Application.ProblemJson;
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
           
            _logger.LogError(exception, "An error occurred while processing the request. Path: {Path}", httpContext.Request.Path.ToString().Sanitize());
 
            return true;
        }
    }
}