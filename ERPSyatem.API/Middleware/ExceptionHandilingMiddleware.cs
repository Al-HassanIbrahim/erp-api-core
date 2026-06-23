using ERPSystem.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ERPSyatem.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Business exception: {Code} - {Message}", ex.Code, ex.Message);
                await HandleExceptionAsync(context, ex.HttpStatusCode, ex.Code, ex.Message);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict detected.");
                await HandleExceptionAsync(context, 409, "CONCURRENCY_CONFLICT", "عفواً، تم تعديل هذه البيانات أو المستند بواسطة مستخدم آخر في نفس اللحظة. يرجى تحديث الصفحة وإعادة المحاولة.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access: {Message}", ex.Message);
                await HandleExceptionAsync(context, 403, "FORBIDDEN", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
                await HandleExceptionAsync(context, 400, "INVALID_OPERATION", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await HandleExceptionAsync(context, 500, "INTERNAL_ERROR", "An unexpected error occurred.");
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, int statusCode, string code, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                error = new
                {
                    code,
                    message
                }
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}