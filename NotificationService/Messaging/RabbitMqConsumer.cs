using System.Text;
using CarDealerShipMicroService.NotificationService.Data;
using CarDealerShipMicroService.NotificationService.Data.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CarDealerShipMicroService.NotificationService.Messaging;

public class RabbitMqConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly string _rabbitMqUrl;
    private IConnection? _connection;
    private IChannel? _channel;

    private const string ExchangeName = "dealership";
    private const string QueueName = "notification-queue";

    private static readonly string[] RoutingKeys =
    [
        "sale.new.created",
        "sale.new.completed",
        "sale.used.created",
        "sale.used.completed",
        "financing.approved",
        "financing.rejected",
        "maintenance.completed",
        "accessory.order.placed",
        "accessory.stock.low"
    ];

    public RabbitMqConsumer(IServiceScopeFactory scopeFactory, ILogger<RabbitMqConsumer> logger, IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _rabbitMqUrl = Environment.GetEnvironmentVariable("RABBITMQ_URL")
                       ?? configuration["RabbitMQ:Url"]
                      ?? "";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAsync();
                _logger.LogInformation("RabbitMQ consumer connected and listening.");

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ connection failed. Retrying in 5 seconds");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ConnectAsync()
    {
        var factory = new ConnectionFactory { Uri = new Uri(_rabbitMqUrl) };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        foreach (var key in RoutingKeys)
        {
            await _channel.QueueBindAsync(
                queue: QueueName,
                exchange: ExchangeName,
                routingKey: key);
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: true,
            consumer: consumer);
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        var routingKey = args.RoutingKey;
        var body = args.Body.ToArray();
        var payloadJson = Encoding.UTF8.GetString(body);

        _logger.LogInformation("Received event: {RoutingKey}", routingKey);

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            EventType = routingKey,
            Payload = payloadJson,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync();

        _logger.LogInformation("Saved the notification {Id} for event {EventType}", notification.Id, notification.EventType);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
            await _channel.CloseAsync();
        if (_connection is not null)
            await _connection.CloseAsync();
        await base.StopAsync(cancellationToken);
    }
}
