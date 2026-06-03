using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace UsedCarSalesService.Services;

public class EventBus
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EventBus> _logger;

    public EventBus(IConfiguration configuration, ILogger<EventBus> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task PublishAsync(string routingKey, object payload)
    {
        var hostName = _configuration["RabbitMq:HostName"];
        if (string.IsNullOrWhiteSpace(hostName))
        {
            _logger.LogWarning("RabbitMq host is not configured. Skipping event publish for {RoutingKey}.", routingKey);
            return Task.CompletedTask;
        }

        var exchangeName = _configuration["RabbitMq:Exchange"] ?? "used-sales.events";
        var userName = _configuration["RabbitMq:UserName"] ?? "guest";
        var password = _configuration["RabbitMq:Password"] ?? "guest";
        var port = int.TryParse(_configuration["RabbitMq:Port"], out var parsedPort) ? parsedPort : 5672;

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            Port = port
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
        channel.BasicPublish(
            exchange: exchangeName,
            routingKey: routingKey,
            basicProperties: null,
            body: body);

        return Task.CompletedTask;
    }
}
