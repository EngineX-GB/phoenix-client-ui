using phoenix_prototype;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly WebSocketService _wsService = new WebSocketService();

    private string _latestStatus;

    private Brush _ingestionStatusColor = Brushes.DarkGray;


    private readonly MainWindow _mainWindow;

    public MainViewModel(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    // colour for service ping status
    public Brush IngestionStatusColor
    {
        get => _ingestionStatusColor;
        set { _ingestionStatusColor = value; OnPropertyChanged(); }
    }

    private Brush _marketStatusColor = Brushes.DarkGray;
    public Brush MarketStatusColor
    {
        get => _marketStatusColor;
        set { _marketStatusColor = value; OnPropertyChanged(); }
    }

    public string LatestStatus
    {
        get => _latestStatus;
        set { _latestStatus = value; OnPropertyChanged(); }
    }

    public async Task InitializeAsync()
    {
        _wsService.OnNotificationReceived += HandleNotification;
        await _wsService.ConnectAsync("ws://localhost:8081/ws");
    }


    private void UpdateColor(ref Brush target, string status, string propertyName)
    {
        switch (status)
        {
            case "OK":
                target = Brushes.Aqua;
                break;

            case "ERROR":
                target = Brushes.Orange;
                break;

            case "STOPPED":
                target = Brushes.LightGray;
                break;
        }

        OnPropertyChanged(propertyName);
    }

    private void HandleNotification(Notification notif)
    {
        Debug.WriteLine($"WS NOTIFICATION RECEIVED: {notif.Content.ServiceName} - {notif.Content.Status}");

        Application.Current.Dispatcher.Invoke(() =>
        {
            switch (notif.Content.ServiceName.ToLower())
            {
                case "ingestion":
                    UpdateColor(ref _ingestionStatusColor, notif.Content.Status, nameof(IngestionStatusColor));
                    break;

                case "market":
                    UpdateColor(ref _marketStatusColor, notif.Content.Status, nameof(MarketStatusColor));
                    break;
            }


            // adding logic here for updating the notification window.

            // Forward to Notifications window
            var entry = new NotificationListEntry
            {
                Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                Operation = notif.Content.Operation,
                Status = notif.Content.Status,
                Detail = notif.Content.Detail
            };

            _mainWindow.AddNotification(entry);

        });
    }




    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
