using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        private readonly RabbitMqLoggingConfig _rabbitMqLoggingConfig;

        public ExceptionLoggingMiddleware(RequestDelegate next,
                                          ILogger<ExceptionLoggingMiddleware> logger,
                                          IRabbitMQPublisher<LogExceptionsEvent> rabbitMQPublisher,
                                          RabbitMqLoggingConfig rabbitMqLoggingConfig)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rabbitMQPublisher = rabbitMQPublisher ?? throw new ArgumentNullException(nameof(rabbitMQPublisher));
            _rabbitMqLoggingConfig = rabbitMqLoggingConfig;
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
                StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError,
                Type = problemDetails.Type,
                Extensions = problemDetails.CustomExtensions
            };

            try
            {
                await _rabbitMQPublisher.PublishMessageAsync(logExceptions, _rabbitMqLoggingConfig.QueueNameLog);

            }
            catch (Exception logEx)
            {
                Console.WriteLine($"Failed to log exception: {logEx.Message}");
            }

            var messageJson = JsonConvert.SerializeObject(logExceptions);

            context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(messageJson);
        }


        private static CustomProblemDetails MapToProblemDetails(Exception exception)
        {

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return exception switch
            {
                NotFoundException nf => new CustomProblemDetails
                {
                    Title = "Not Found",
                    Detail = nf.Message,
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                },
                UnauthorizedAccessException ua => new CustomProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = ua.Message,
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
                },
                ValidationException ve => new CustomProblemDetails
                {
                    Title = "Validation Errors",
                    Detail = ve.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    CustomExtensions = new Dictionary<string, object?>
                    {
                        { "errors", ve.Errors }
                    }

                },
                ArgumentNullException an => new CustomProblemDetails
                {
                    Title = "Bad Request",
                    Detail = an.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                },
                _ => new CustomProblemDetails
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
