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
        using var connection = CreateConnection();
        if (connection is null)
        {
            _logger.LogWarning("RabbitMQ connection is not configured. Skipping event publish for {RoutingKey}.", routingKey);
            return Task.CompletedTask;
        }

        var exchangeName = _configuration["RabbitMq:Exchange"] ?? "dealership";
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);
        var envelope = new
        {
            @event = routingKey,
            timestamp = DateTime.UtcNow.ToString("o"),
            data = payload
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));
        channel.BasicPublish(
            exchange: exchangeName,
            routingKey: routingKey,
            basicProperties: null,
            body: body);

        return Task.CompletedTask;
    }

    private IConnection? CreateConnection()
    {
        try
        {
            var rabbitUrl = _configuration["RABBITMQ_URL"];
            if (!string.IsNullOrWhiteSpace(rabbitUrl))
            {
                var urlFactory = new ConnectionFactory { Uri = new Uri(rabbitUrl) };
                return urlFactory.CreateConnection();
            }

            var hostName = _configuration["RabbitMq:HostName"];
            if (string.IsNullOrWhiteSpace(hostName))
            {
                return null;
            }

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

            return factory.CreateConnection();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RabbitMQ connection.");
            return null;
        }
    }
}
