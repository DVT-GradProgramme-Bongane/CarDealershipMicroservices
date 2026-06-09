using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Inventory.Api.Services;

public class RabbitMqPublisher : IAsyncDisposable
{
    private readonly IConnection _conn;
    private readonly IChannel _channel;
    private const string Exchange = "dealership";

    private RabbitMqPublisher(IConnection conn, IChannel channel)
    {
        _conn = conn;
        _channel = channel;
    }

    public static async Task<RabbitMqPublisher> CreateAsync(IConfiguration config)
    {
        var factory = new ConnectionFactory
        {
            Uri= new Uri(config["RABBITMQ_URL"] ?? "localhost")
        };
        var conn    = await factory.CreateConnectionAsync();
        var channel = await conn.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: Exchange,
            type: ExchangeType.Topic,
            durable: true);

        return new RabbitMqPublisher(conn, channel);
    }

    public async Task PublishAsync(string routingKey, object payload)
    {
        var envelope = new {
            @event    = routingKey,
            timestamp = DateTime.UtcNow,
            data      = payload
        };
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));

        await _channel.BasicPublishAsync(
            exchange: Exchange,
            routingKey: routingKey,
            body: body);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _conn.CloseAsync();
        GC.SuppressFinalize(this);
    }
}
