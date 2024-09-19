using Refit;
using RequestLoggingMiddlewareLib.Models;

namespace RequestLoggingMiddlewareLib.Interface
{
    public interface IMessageBrokerLog
    {
        [Post("/api/LogExceptions/CreateLogUserAPI")]
        Task CreateLogUserAPI([Body] LogExceptionsEvent logEvent);

        [Post("/api/Trace/CreateTraceUserAPI")]
        Task CreateTraceUserAPI([Body] TraceRequestEvent logTrace);
    }
}
