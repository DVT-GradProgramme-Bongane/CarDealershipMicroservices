using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace NewCarSalesService.Messaging;

public class EventPublisher
{
    private readonly ConnectionFactory _factory;

    public EventPublisher(IConfiguration configuration)
    {
        var rabbitUrl = configuration["RABBITMQ_URL"] ?? "amqp://rabbitmq:5672";
        _factory = new ConnectionFactory { Uri = new Uri(rabbitUrl) };
    }

    public async Task PublishSaleEventAsync(string eventType, Guid saleId, Guid carId, Guid clientId)
    {
        await using var connection = await _factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: "dealership",
            type: ExchangeType.Topic,
            durable: true
        );

        var payload = new
        {
            @event = eventType,
            timestamp = DateTime.UtcNow.ToString("o"),
            data = new { sale_id = saleId, car_id = carId, client_id = clientId }
        };

        var json = JsonSerializer.Serialize(payload);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: "dealership",
            routingKey: eventType,
            body: body
        );
    }
}