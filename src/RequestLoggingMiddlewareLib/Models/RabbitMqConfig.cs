namespace RequestLoggingMiddlewareLib.Models
{
    public class RabbitMqConfig
    {
        public string ConnectionString { get; set; } = null!;
        public string QueueName { get; set; } = null!;
    }
}
