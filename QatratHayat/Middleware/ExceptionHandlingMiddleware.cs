using System.Net;
using System.Text.Json;
using QatratHayat.Application.Common.Exceptions;

namespace QatratHayat.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionHandlingMiddleware> logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate _next,
            ILogger<ExceptionHandlingMiddleware> _logger)
        {
            next = _next;
            logger = _logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Continue request processing through the next middleware in the pipeline.
                await next(context);
            }
            catch (BadRequestException ex)
            // Expected client-side/business validation error.
            {
                logger.LogWarning(ex, "Bad request exception occurred.");
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, "Bad Request", ex.Message, ex.Errors);
            }
            catch (UnauthorizedException ex)
            {
                // Authentication/authorization related failure.
                logger.LogWarning(ex, "Unauthorized exception occurred.");
                await HandleExceptionAsync(context, HttpStatusCode.Unauthorized, "Unauthorized", ex.Message);
            }
            catch (NotFoundException ex)
            {
                // Requested resource does not exist.
                logger.LogWarning(ex, "Not found exception occurred.");
                await HandleExceptionAsync(context, HttpStatusCode.NotFound, "Not Found", ex.Message);
            }
            catch (ConflictException ex)
            {
                // Conflict usually means duplicate or state conflict.
                logger.LogWarning(ex, "Conflict exception occurred.");
                await HandleExceptionAsync(context, HttpStatusCode.Conflict, "Conflict", ex.Message);
            }
            catch (Exception ex)
            {
                // Unexpected server-side error.
                logger.LogError(ex, "Unhandled exception occurred.");
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "Server Error", "An unexpected error occurred.");
            }
        }

        private static async Task HandleExceptionAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            string title,
            string message,
            List<string>? errors = null)
        {
            // Set response type and HTTP status code.
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            // Create a consistent JSON error shape for frontend consumption.
            var response = new ErrorResponse
            {
                Title = title,
                Status = (int)statusCode,
                Message = message,
                Errors = errors ?? new List<string>()
            };

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }

    public class ErrorResponse
    {
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
    }
}