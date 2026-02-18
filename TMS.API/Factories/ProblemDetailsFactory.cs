using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TMS.Application.Interfaces.Factories;


namespace TMS.API.Factories
{
    internal sealed class ProblemDetailsFactory : IProblemDetailsFactory
    {
        private readonly ILogger<ProblemDetailsFactory> _logger;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.API.Factories.ProblemDetailsFactory");

        public ProblemDetailsFactory(ILogger<ProblemDetailsFactory> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ProblemDetails CreateProblemDetails(string title, string detail, int statusCode)
        {
            using Activity? activity = _activitySource.StartActivity("ProblemDetailsFactory.CreateProblemDetails");

            _logger.LogDebug("Creating ProblemDetails with StatusCode {StatusCode}, Title {Title}", statusCode, title);

            if (!Enum.IsDefined(typeof(HttpStatusCode), statusCode) && statusCode != StatusCodes.Status499ClientClosedRequest)
            {
                _logger.LogError("Invalid status code provided to CreateProblemDetails: {StatusCode}", statusCode);
                throw new InvalidOperationException("Must provide valid status code");
            }

            string type = statusCode == StatusCodes.Status499ClientClosedRequest ? "https://developers.cloudflare.com/support/troubleshooting/http-status-codes/4xx-client-error/error-499/" : $"https://datatracker.ietf.org/doc/html/rfc9110#status.{statusCode}";
            
            ProblemDetails problemDetails = new ProblemDetails
            {
                Title = title,
                Detail = detail,
                Status = statusCode,
                Type = type
            };

            _logger.LogInformation("ProblemDetails created successfully with StatusCode {StatusCode}", statusCode);
            
            return problemDetails;
        }

        public ValidationProblemDetails CreateValidationProblemDetails(ModelStateDictionary modelState, HttpStatusCode httpStatusCode)
        {
            using Activity? activity = _activitySource.StartActivity("ProblemDetailsFactory.CreateValidationProblem");

            _logger.LogDebug("Creating ValidationProblemDetails with StatusCode {StatusCode} and {ErrorCount} validation errors", (int)httpStatusCode, modelState.ErrorCount);

            ValidationProblemDetails validationProblemDetails = new ValidationProblemDetails(modelState)
            {
                Type = $"https://datatracker.ietf.org/doc/html/rfc9110#status.{(int)httpStatusCode}"
            };

            _logger.LogInformation("ValidationProblemDetails created with StatusCode {StatusCode}",(int)httpStatusCode);

            return validationProblemDetails;
        }
    }
}