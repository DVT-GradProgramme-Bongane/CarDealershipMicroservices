using System.Threading;
using System.Threading.Tasks;

namespace AccessoriesSuppliersService.Messaging;

public interface IRabbitMqPublisher
{
    Task PublishAsync(string routingKey, object message, CancellationToken cancellationToken);
}
