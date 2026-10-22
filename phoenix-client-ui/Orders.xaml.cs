using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shapes;

namespace phoenix_prototype
{
    /// <summary>
    /// Interaction logic for Orders.xaml
    /// </summary>
    public partial class Orders : Window
    {

        private readonly AppDataService _data;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
    IntPtr hwnd,
    int attr,
    ref int attrValue,
    int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_BORDER_COLOR = 34;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = new WindowInteropHelper(this).Handle;

            // Enable dark mode frame (prevents white border)
            int dark = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));

            // Set border colour (ARGB)
            int borderColor = unchecked((int)0xFF1A1A1A); // match your window background
            DwmSetWindowAttribute(hwnd, DWMWA_BORDER_COLOR, ref borderColor, sizeof(int));
        }
        //public ObservableCollection<OrderEntry> OrderEntries { get; set; } = new ObservableCollection<OrderEntry>();


        private WindowConfig windowConfig { get; set; } = new WindowConfig();
        public Orders(AppDataService data)
        {
            InitializeComponent();
            _data = data;
            DataContext = _data;
        }

        public async Task LoadAllOrdersAsync()
        {
            using var client = new HttpClient();

            // Replace with your real API endpoint
            string url = "http://localhost:8081/orders/all";

            var response = await client.GetAsync(url);
            Console.WriteLine(response.Content);

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            var items = JsonSerializer.Deserialize<List<OrderEntry>>(json);

            _data.OrderEntries.Clear();
            foreach (var item in items)
                _data.OrderEntries.Add(item);
        }

        public bool CancelOrder(string orderId)
        {
            using var client = new HttpClient();
            string url = "http://localhost:8081/orders/cancel?id=" + orderId;
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            var response = client.Send(request);
            return response.IsSuccessStatusCode;
        }


        // Make the REST call when the window opens
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        { 
            await LoadAllOrdersAsync();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) { this.Close(); }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (!windowConfig._lockEnabled)
                    this.DragMove();

            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e) { await LoadAllOrdersAsync(); }

        private void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridOrders.SelectedItem is OrderEntry selectedOrder)
            {
                Console.WriteLine($"Cancelling order for user: {selectedOrder.Id}");
                bool cancelOrderResult = CancelOrder(selectedOrder.Id.ToString());
                if (cancelOrderResult)
                {
                    selectedOrder.Status = "CANCELLED";
                }
                else
                {
                    Console.WriteLine("No row selected.");
                }
            }
        }


        private async void ImportOrders_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select file to import",
                Filter = "All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;

                try
                {
                    await UploadFileAsync(filePath);
                    await LoadAllOrdersAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Upload failed: " + ex.Message);
                }
            }
        }

        private async Task UploadFileAsync(string filePath)
        {
            using var client = new HttpClient();
            using var form = new MultipartFormDataContent();

            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var fileContent = new ByteArrayContent(fileBytes);

            // IMPORTANT: "myFile" must match your server's expected field name
            form.Add(fileContent, "file", System.IO.Path.GetFileName(filePath));

            var response = await client.PostAsync("http://localhost:8081/orders/import", form);
            response.EnsureSuccessStatusCode();
        }

        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            IsClosed = true;
            base.OnClosed(e);
        }


        private void LockButton_Click(object sender, RoutedEventArgs e)
        {
            if (windowConfig != null)
            {
                if (windowConfig._lockEnabled)
                {
                    // if it's already locked, then unlock it:
                    this.ResizeMode = ResizeMode.CanResize;
                    windowConfig._lockEnabled = false;
                }
                else
                {
                    this.ResizeMode = ResizeMode.NoResize;
                    windowConfig._lockEnabled = true;
                }
            }
        }


    }
}
