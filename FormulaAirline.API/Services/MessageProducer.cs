using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace FormulaAirline.API.Services;

public class MessageProducer : IMessageProducer
{
    public void SendMessage<T>(T message)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "user",
            Password = "mypass",
            VirtualHost = "/"
        };
        using var conn = factory.CreateConnection();
        using var channel = conn.CreateModel();
        channel.QueueDeclare(queue: "bookings", durable: true, exclusive: false);
        var jsonString = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(jsonString));
        channel.BasicPublish("", "bookings", body: body);
    }
}