using Newtonsoft.Json;
using RabbitMQ.Client;
using RequestLoggingMiddlewareLib.Interface;
using RequestLoggingMiddlewareLib.Models;
using System.Text;

namespace RequestLoggingMiddlewareLib.RabbitMQ
{
    public class RabbitMqPublisher<T> : IRabbitMQPublisher<T>
    {
        private readonly RabbitMqLoggingConfig _rabbitMqLoggingConfig;

        public RabbitMqPublisher(RabbitMqLoggingConfig rabbitMqLoggingConfig)
        {
            _rabbitMqLoggingConfig = rabbitMqLoggingConfig;
        }

        public async Task PublishMessageAsync(T message, string queueName)
        {

            var factory = new ConnectionFactory() { Uri = new Uri(_rabbitMqLoggingConfig.ConnectionString) };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var messageJson = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(messageJson);


                await Task.Run(() => channel.BasicPublish(exchange: "",
                                     routingKey: queueName,
                                     basicProperties: null,
                                     body: body));

            }


        }
    }
}
