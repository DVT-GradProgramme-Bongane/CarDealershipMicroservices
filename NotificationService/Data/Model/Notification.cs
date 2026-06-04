using System.ComponentModel.DataAnnotations.Schema;

namespace CarDealerShipMicroService.NotificationService.Data.Model;

[Table("log", Schema = "notifications")]
public class Notification
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("event_type")]
    public string EventType { get; set; } = string.Empty;

    [Column("payload")]
    public string Payload { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
