using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class WebSocketService
{
    private readonly ClientWebSocket _socket = new ClientWebSocket();
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public event Action<Notification> OnNotificationReceived;

    public async Task ConnectAsync(string url)
    {
        await _socket.ConnectAsync(new Uri(url), CancellationToken.None);
        _ = ReceiveLoop();
    }

    private async Task ReceiveLoop()
    {
        var buffer = new byte[4096];

        while (_socket.State == WebSocketState.Open)
        {
            var result = await _socket.ReceiveAsync(buffer, _cts.Token);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                break;
            }

            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

            var notification = JsonSerializer.Deserialize<Notification>(json, JsonOptions);

            OnNotificationReceived?.Invoke(notification);
        }
    }
}
