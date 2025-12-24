using System.Diagnostics;

namespace TMS.API.Middleware
{
    public sealed class TraceMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.API.Middleware.TraceMiddleware");

        public TraceMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using (Activity? activity = _activitySource.StartActivity("TraceMiddleware.InvokeAsync"))
            {
                string traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

                context.Response.OnStarting(() =>
                {
                    context.Response.Headers["X-Trace-Id"] = traceId;
                    return Task.CompletedTask;
                });   
            }
            
            await _next(context);
        }
    }
}