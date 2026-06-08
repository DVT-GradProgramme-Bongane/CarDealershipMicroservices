using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

public class FinancingEventPublisher : IFinancingEventPublisher
{
    private readonly IConnectionFactory _factory;

    public FinancingEventPublisher(IConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task PublishApprovedAsync(Guid applicationId, Guid saleId, Guid clientId, CancellationToken ct) =>
        await PublishAsync("financing.approved", applicationId, saleId, clientId, ct);

    public async Task PublishRejectedAsync(Guid applicationId, Guid saleId, Guid clientId, CancellationToken ct) =>
        await PublishAsync("financing.rejected", applicationId, saleId, clientId, ct);

    private async Task PublishAsync(string routingKey, Guid applicationId, Guid saleId, Guid clientId, CancellationToken ct)
    {
        await using var connection = await _factory.CreateConnectionAsync(ct);
        await using var channel = await connection.CreateChannelAsync(null, ct);

        await channel.ExchangeDeclareAsync(
            exchange: "dealership",
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: ct
        );

        var payload = new
        {
            @event = routingKey,
            timestamp = DateTime.UtcNow.ToString("o"),
            data = new { application_id = applicationId, sale_id = saleId, client_id = clientId }
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));

        await channel.BasicPublishAsync(
            exchange: "dealership",
            routingKey: routingKey,
            body: body,
            cancellationToken: ct
        );
    }
}