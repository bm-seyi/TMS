using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TMS.Core.Extensions;
using TMS.Core.Interfaces.Factories;


namespace TMS.API.ExceptionHandlers
{
    public sealed class OperationCanceledHandler : IExceptionHandler
    {
        private readonly ILogger<OperationCanceledHandler> _logger;
        private readonly IProblemDetailsWriter _problemDetailsWriter;
        private readonly IProblemDetailsFactory _problemDetailsFactory;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.API.ExceptionHandlers.OperationCanceledHandler");
 
        public OperationCanceledHandler(ILogger<OperationCanceledHandler> logger, IProblemDetailsWriter problemDetailsWriter, IProblemDetailsFactory problemDetailsFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _problemDetailsWriter = problemDetailsWriter ?? throw new ArgumentNullException(nameof(problemDetailsWriter));
            _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
        }
 
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            using Activity? activity = _activitySource.StartActivity("OperationCanceledHandler.TryHandleAsync");
 
            if (exception is OperationCanceledException)
            {
                ProblemDetails problemDetails = _problemDetailsFactory.CreateProblemDetails("Request Cancelled", "The request was cancelled, possibly due to a timeout or client disconnect.", StatusCodes.Status499ClientClosedRequest);
 
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