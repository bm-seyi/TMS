using System.Diagnostics;
using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;


namespace TMS.API.ExceptionHandlers
{
    public sealed class OperationCanceledHandler : IExceptionHandler
    {
        private readonly ILogger<OperationCanceledHandler> _logger;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.API.ExceptionHandlers.OperationCanceledHandler");
 
        public OperationCanceledHandler(ILogger<OperationCanceledHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
 
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            using Activity? activity = _activitySource.StartActivity("OperationCanceledHandler.TryHandleAsync");
 
            if (exception is OperationCanceledException)
            {
                ProblemDetails problemDetails = new ProblemDetails
                {
                    Title = "Request Cancelled",
                    Detail = "The request was cancelled, possibly due to a timeout or client disconnect.",
                    Status = StatusCodes.Status499ClientClosedRequest,
                    Type = "https://developers.cloudflare.com/support/troubleshooting/http-status-codes/4xx-client-error/error-499/%22"
                };
 
 
                httpContext.Response.ContentType = MediaTypeNames.Application.ProblemJson;
                httpContext.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
                await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
 
                _logger.LogError(exception, "Operation was canceled by the user. Request Path: {RequestPath}, Method: {RequestMethod}, TraceIdentifier: {TraceId}", httpContext.Request.Path, httpContext.Request.Method, httpContext.TraceIdentifier);
 
                return true;
            }
 
            return false;
        }
    }
}