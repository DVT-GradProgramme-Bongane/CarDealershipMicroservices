using System.Text.Json.Serialization;

public class SaleCreatedMessage
{
    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public SaleCreatedData Data { get; set; } = new();
}

public class SaleCreatedData
{
    [JsonPropertyName("sale_id")]
    public Guid SaleId { get; set; }

    [JsonPropertyName("car_id")]
    public Guid CarId { get; set; }

    [JsonPropertyName("client_id")]
    public Guid ClientId { get; set; }
}