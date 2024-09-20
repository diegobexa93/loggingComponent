using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RequestLoggingMiddlewareLib.Interface;
using RequestLoggingMiddlewareLib.Middleware;
using RequestLoggingMiddlewareLib.Models;
using RequestLoggingMiddlewareLib.RabbitMQ;

namespace RequestLoggingMiddlewareLib.Extensions
{
    public static class RabbitMqLoggingServiceExtensions
    {
        public static IServiceCollection AddRabbitMqLoggingServices(this IServiceCollection services,
                                                                    IConfiguration configuration)
        {
            // Register RabbitMQPublisher with DI container
            services.AddSingleton(typeof(IRabbitMQPublisher<>), typeof(RabbitMqPublisher<>));

            // Bind RabbitMqLoggingConfig from configuration
            var rabbitMqLoggingConfig = new RabbitMqLoggingConfig();
            configuration.GetSection("RabbitMqLoggingConfig").Bind(rabbitMqLoggingConfig);
            services.AddSingleton(rabbitMqLoggingConfig);

            return services;

        }

        public static IApplicationBuilder UseRequestLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }

        public static IApplicationBuilder UseExceptionLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionLoggingMiddleware>();
        }
    }
}
