using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Refit;
using RequestLoggingMiddlewareLib.Exceptions;
using RequestLoggingMiddlewareLib.Interface;
using RequestLoggingMiddlewareLib.Models;

namespace RequestLoggingMiddlewareLib.Middleware
{
    public class ExceptionLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionLoggingMiddleware> _logger;
        private readonly IRabbitMQPublisher<LogExceptionsEvent> _rabbitMQPublisher;

        public ExceptionLoggingMiddleware(RequestDelegate next,
                                          ILogger<ExceptionLoggingMiddleware> logger,
                                          IRabbitMQPublisher<LogExceptionsEvent> rabbitMQPublisher)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rabbitMQPublisher = rabbitMQPublisher ?? throw new ArgumentNullException(nameof(rabbitMQPublisher));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var problemDetails = MapToProblemDetails(exception);

            // Create a log entry based on the exception
            var logExceptions = new LogExceptionsEvent
            {
                Title = problemDetails.Title,
                Detail = problemDetails.Detail,
                StatusCode = problemDetails.Status,
                Type = problemDetails.Type,
                Extensions = problemDetails.Extensions
            };

            try
            {
                await _rabbitMQPublisher.PublishMessageAsync(logExceptions);

            }
            catch (Exception logEx)
            {
                Console.WriteLine($"Failed to log exception: {logEx.Message}");
            }

            var messageJson = JsonConvert.SerializeObject(logExceptions);

            context.Response.StatusCode = problemDetails.Status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(messageJson);
        }


        private static ProblemDetails MapToProblemDetails(Exception exception)
        {

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return exception switch
            {
                NotFoundException nf => new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = nf.Message,
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                },
                UnauthorizedAccessException ua => new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = ua.Message,
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
                },
                ValidationException ve => new ProblemDetails
                {
                    Title = "Validation Errors",
                    Detail = ve.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Extensions = new Dictionary<string, object?>
                    {
                        { "errors", ve.Errors }
                    }
                },
                ArgumentNullException an => new ProblemDetails
                {
                    Title = "Bad Request",
                    Detail = an.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                },
                _ => new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = exception.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                }
            };
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }
    }
}
