using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RequestLoggingMiddlewareLib.Interface;

namespace RequestLoggingMiddlewareLib
{
    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }

        public static IServiceCollection AddRequestLogging(this IServiceCollection services, Func<IServiceProvider, IMessageBrokerLog> implementationFactory)
        {
            return services.AddScoped(implementationFactory);
        }
    }
}
