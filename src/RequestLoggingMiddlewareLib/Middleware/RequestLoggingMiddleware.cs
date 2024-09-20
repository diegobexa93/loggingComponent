using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using RequestLoggingMiddlewareLib.Interface;
using RequestLoggingMiddlewareLib.Models;
using System.Text;


namespace RequestLoggingMiddlewareLib.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly IRabbitMQPublisher<TraceRequestEvent> _rabbitMQPublisher;
        private readonly RabbitMqLoggingConfig _rabbitMqLoggingConfig;

        public RequestLoggingMiddleware(RequestDelegate next,
                                        ILogger<RequestLoggingMiddleware> logger,
                                        IRabbitMQPublisher<TraceRequestEvent> rabbitMQPublisher,
                                        RabbitMqLoggingConfig rabbitMqLoggingConfig)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rabbitMQPublisher = rabbitMQPublisher ?? throw new ArgumentNullException(nameof(rabbitMQPublisher));
            _rabbitMqLoggingConfig = rabbitMqLoggingConfig;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestId = Guid.NewGuid();
            var requestTimestamp = DateTime.UtcNow;
            var traceRequestEvent = new TraceRequestEvent(requestId, requestTimestamp);

            await LogRequest(context, traceRequestEvent);

            // Capture the original response body stream
            var originalResponseBody = context.Response.Body;

            try
            {
                using (var responseBody = new MemoryStream())
                {
                    context.Response.Body = responseBody;

                    // Continue the pipeline
                    await _next(context);

                    await LogResponseDetails(context, traceRequestEvent);

                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalResponseBody);
                }

                try
                {
                    await _rabbitMQPublisher.PublishMessageAsync(traceRequestEvent, _rabbitMqLoggingConfig.QueueNameTrace);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending message to RabbitMQ");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging the request/response");
                throw; // Re-throw the exception to let the pipeline handle it
            }
            finally
            {
                context.Response.Body = originalResponseBody; // Restore the original response body
            }
        }

        private async Task LogRequest(HttpContext context, TraceRequestEvent traceRequestEvent)
        {
            context.Request.EnableBuffering(); // Enable request body re-reading

            traceRequestEvent.RequestURL = context.Request.GetDisplayUrl();

            // Capture request headers
            foreach (var header in context.Request.Headers)
            {
                traceRequestEvent.RequestHeaders[header.Key] = header.Value.ToString();
            }

            // Capture request body
            traceRequestEvent.RequestBody = await ReadRequestBodyAsync(context);

            context.Request.Body.Position = 0; // Reset the body stream position for further use by other middlewares
        }

        private async Task<string> ReadRequestBodyAsync(HttpContext context)
        {
            if (context.Request.ContentLength == null || context.Request.ContentLength <= 0)
                return string.Empty;

            context.Request.EnableBuffering();
            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; // Reset stream position after reading
                return body;
            }
        }

        private async Task LogResponseDetails(HttpContext context, TraceRequestEvent traceRequestEvent)
        {
            traceRequestEvent.TraceResponse.ResponseStatusCode = context.Response.StatusCode;

            foreach (var header in context.Response.Headers)
            {
                traceRequestEvent.TraceResponse.ResponseHeaders[header.Key] = header.Value.ToString();
            }

            // Capture response body
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            traceRequestEvent.TraceResponse.ResponseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
        }
    }
}
