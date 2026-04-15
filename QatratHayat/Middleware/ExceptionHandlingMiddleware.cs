using QatratHayat.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

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
                await next(context);
            }
            catch (BadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request exception occurred.");
                await HandleExceptionAsync(
                    context,
                    HttpStatusCode.BadRequest,
                    "Bad Request",
                    ex.Message,
                    ex.Code,
                    ex.Errors);
            }
            catch (UnauthorizedException ex)
            {
                logger.LogWarning(ex, "Unauthorized exception occurred.");
                await HandleExceptionAsync(
                    context,
                    HttpStatusCode.Unauthorized,
                    "Unauthorized",
                    ex.Message,
                    ex.Code);
            }
            catch (NotFoundException ex)
            {
                logger.LogWarning(ex, "Not found exception occurred.");
                await HandleExceptionAsync(
                    context,
                    HttpStatusCode.NotFound,
                    "Not Found",
                    ex.Message,
                    ex.Code);
            }
            catch (ConflictException ex)
            {
                logger.LogWarning(ex, "Conflict exception occurred.");
                await HandleExceptionAsync(
                    context,
                    HttpStatusCode.Conflict,
                    "Conflict",
                    ex.Message,
                    ex.Code);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception occurred.");
                await HandleExceptionAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    "Server Error",
                    "An unexpected error occurred.",
                    ErrorCodes.InternalServerError);
            }
        }

        private static async Task HandleExceptionAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            string title,
            string message,
            string code,
            List<string>? errors = null)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new ErrorResponse
            {
                Title = title,
                Status = (int)statusCode,
                Message = message,
                Code = code,
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
        public string Code { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
    }
}