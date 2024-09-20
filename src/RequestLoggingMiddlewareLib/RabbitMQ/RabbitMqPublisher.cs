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

        public async Task PublishMessageAsync(T message)
        {

            var factory = new ConnectionFactory() { Uri = new Uri(_rabbitMqLoggingConfig.ConnectionString) };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: _rabbitMqLoggingConfig.QueueNameTrace,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var messageJson = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(messageJson);


                await Task.Run(() => channel.BasicPublish(exchange: "",
                                     routingKey: _rabbitMqLoggingConfig.QueueNameTrace,
                                     basicProperties: null,
                                     body: body));

            }


        }
    }
}
