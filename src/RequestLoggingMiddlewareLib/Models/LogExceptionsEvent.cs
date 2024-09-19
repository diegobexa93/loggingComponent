namespace RequestLoggingMiddlewareLib.Models
{
    public class LogExceptionsEvent : IntegrationBaseEvent
    {
        public string? Title { get; set; }
        public string? Detail { get; set; }
        public int StatusCode { get; set; }
        public string? Type { get; set; }
        public IDictionary<string, object?>? Extensions { get; set; }
    }
}
