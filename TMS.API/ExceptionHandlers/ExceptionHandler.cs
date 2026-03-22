using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using TMS.Application.Extensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;


namespace TMS.API.ExceptionHandlers
{
    internal sealed class ExceptionHandler(ILogger<ExceptionHandler> logger, IProblemDetailsWriter problemDetailsWriter, ProblemDetailsFactory problemDetailsFactory) : IExceptionHandler
    {
        private readonly ILogger<ExceptionHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IProblemDetailsWriter _problemDetailsWriter = problemDetailsWriter ?? throw new ArgumentNullException(nameof(problemDetailsWriter));
        private readonly ProblemDetailsFactory _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.API");

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            using Activity? _  =_activitySource.StartActivity("ExceptionHandler.TryHandleAsync");

            ProblemDetails problemDetails = _problemDetailsFactory.CreateProblemDetails(httpContext, StatusCodes.Status500InternalServerError);
            
            ProblemDetailsContext problemDetailsContext = new ProblemDetailsContext()
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            };
         
            await _problemDetailsWriter.WriteAsync(problemDetailsContext);
            
            _logger.LogError(exception, "An error occurred while processing the request. Path: {Path}", httpContext.Request.Path.ToString().Sanitize());

            return true;
        }
    }
}