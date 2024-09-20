namespace RequestLoggingMiddlewareLib.Interface
{
    public interface IRabbitMQPublisher<T>
    {
        Task PublishMessageAsync(T message, string queueName);
    }
}
