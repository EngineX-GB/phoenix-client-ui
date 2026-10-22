using System.Text.Json.Serialization;

public class Notification
{
    [JsonPropertyName("notificationType")]
    public string NotificationType { get; set; }

    [JsonPropertyName("content")]
    public Content Content { get; set; }
}

public class Content
{
    [JsonPropertyName("operation")]
    public string Operation { get; set; }

    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("detail")]
    public string Detail { get; set; }
}
