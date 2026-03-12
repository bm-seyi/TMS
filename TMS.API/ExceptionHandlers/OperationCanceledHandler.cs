using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Diagnostics;
using TMS.Application.Extensions;


namespace TMS.API.ExceptionHandlers
{
    internal sealed class OperationCanceledHandler : IExceptionHandler
    {
        private readonly ILogger<OperationCanceledHandler> _logger;
        private readonly IProblemDetailsWriter _problemDetailsWriter;
        private readonly ProblemDetailsFactory _problemDetailsFactory;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.API");
 
        public OperationCanceledHandler(ILogger<OperationCanceledHandler> logger, IProblemDetailsWriter problemDetailsWriter, ProblemDetailsFactory problemDetailsFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _problemDetailsWriter = problemDetailsWriter ?? throw new ArgumentNullException(nameof(problemDetailsWriter));
            _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
        }
 
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            using Activity? _ = _activitySource.StartActivity("OperationCanceledHandler.TryHandleAsync");
            
            if (exception is OperationCanceledException)
            {
                ProblemDetails problemDetails = _problemDetailsFactory.CreateProblemDetails(httpContext,  StatusCodes.Status499ClientClosedRequest, "Request Cancelled", "https://developers.cloudflare.com/support/troubleshooting/http-status-codes/4xx-client-error/error-499/", "The request was cancelled, possibly due to a timeout or client disconnect.");

                ProblemDetailsContext problemDetailsContext = new ProblemDetailsContext()
                {
                    HttpContext = httpContext,
                    ProblemDetails = problemDetails
                };

                await _problemDetailsWriter.WriteAsync(problemDetailsContext);

                _logger.LogError(exception, "Operation was canceled by the user. Request Path: {RequestPath}, Method: {RequestMethod}, TraceIdentifier: {TraceId}", httpContext.Request.Path.ToString().Sanitize(), httpContext.Request.Method.Sanitize(), httpContext.TraceIdentifier);
                return true;
            }

            return false;
        }
    }
}