using System.Text.Json;
using RabbitMQ.Client;

namespace MaintenanceService.Services;

public class MaintenanceEventPublisher
{
    private const string ExchangeName = "dealership";
    private const string CompletedRoutingKey = "maintenance.completed";

    private readonly ConnectionFactory _factory;

    public MaintenanceEventPublisher(IConfiguration configuration)
    {
        var rabbitUrl = configuration["RABBITMQ_URL"] ?? "amqp://rabbitmq:5672";
        _factory = new ConnectionFactory { Uri = new Uri(rabbitUrl) };
    }

    public async Task PublishCompletedAsync(
        Guid jobId,
        Guid carId,
        Guid clientId,
        Guid staffId,
        CancellationToken cancellationToken)
    {
        await using var connection = await _factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: cancellationToken);

        var payload = new
        {
            @event = CompletedRoutingKey,
            timestamp = DateTime.UtcNow.ToString("o"),
            data = new
            {
                job_id = jobId,
                car_id = carId,
                client_id = clientId,
                staff_id = staffId
            }
        };

        var body = JsonSerializer.SerializeToUtf8Bytes(payload);

        await channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: CompletedRoutingKey,
            body: body,
            cancellationToken: cancellationToken);
    }
}
