namespace RequestLoggingMiddlewareLib.Models
{
    public class RabbitMqLoggingConfig
    {
        public string ConnectionString { get; set; } = null!;
        public string QueueNameTrace { get; set; } = null!;
        public string QueueNameLog { get; set; } = null!;

    }
}
