using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Mime;
using System.Text.Json;


namespace TMS.Core.Extensions
{
    public static class HttpResponseExtension
    {
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Core.Extensions.HttpResponseExtension");

        extension (HttpResponse httpResponse)
        {
            public async Task WriteAsJsonAsync(ProblemDetails problemDetails, CancellationToken cancellationToken = default)
            {
                using Activity? activity = _activitySource.StartActivity("HttpResponseExtension.WriteAsJsonAsync");
                
                if (string.IsNullOrEmpty(httpResponse.ContentType))
                    httpResponse.ContentType = MediaTypeNames.Application.Json;

                await JsonSerializer.SerializeAsync(httpResponse.Body, problemDetails, cancellationToken: cancellationToken);
            }
        }
    }
}