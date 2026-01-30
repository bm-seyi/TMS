using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using TMS.Core.Extensions;
using TMS.Core.Interfaces.Factories;


namespace TMS.API.ExceptionHandlers
{
    public sealed class ExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ExceptionHandler> _logger;
        private readonly IProblemDetailsWriter _problemDetailsWriter;
        private readonly IProblemDetailsFactory _problemDetailsFactory;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.API.ExceptionHandlers.ExceptionHandler");
 
        public ExceptionHandler(ILogger<ExceptionHandler> logger, IProblemDetailsWriter problemDetailsWriter, IProblemDetailsFactory problemDetailsFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _problemDetailsWriter = problemDetailsWriter ?? throw new ArgumentNullException(nameof(problemDetailsWriter));
            _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            using Activity? activity = _activitySource.StartActivity("ExceptionHandler.TryHandleAsync");
 
            ProblemDetails problemDetails = _problemDetailsFactory.CreateProblemDetails("An unexpected error occurred.", "Something went wrong while processing your request.", StatusCodes.Status500InternalServerError);
 
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