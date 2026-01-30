using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TMS.Core.Interfaces.Factories
{
    public interface IProblemDetailsFactory
    {
        ProblemDetails CreateProblemDetails(string title, string detail, int statusCode);
        ValidationProblemDetails CreateValidationProblemDetails(ModelStateDictionary modelState, HttpStatusCode httpStatusCode);
    }
}