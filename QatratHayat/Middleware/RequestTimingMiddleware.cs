using System.Diagnostics;

namespace QatratHayat.API.Middleware
{
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTimingMiddleware> _logger;

        public RequestTimingMiddleware(
            RequestDelegate next,
            ILogger<RequestTimingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            context.Response.OnStarting(() =>
            {
                stopwatch.Stop();

                var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;

                context.Response.Headers["X-Response-Time-Seconds"] =
                    elapsedSeconds.ToString("F2");

                _logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedSeconds:F2} seconds",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    elapsedSeconds
                );

                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}