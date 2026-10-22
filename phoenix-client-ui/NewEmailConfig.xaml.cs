using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace phoenix_prototype
{
    /// <summary>
    /// Interaction logic for NewEmailConfig.xaml
    /// </summary>
    public partial class NewEmailConfig : Window
    {
        public NewEmailConfig()
        {
            InitializeComponent();
        }

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

        private async void AddEmailConfig_Click(object sender, RoutedEventArgs e)
        {
            await AddEmailConfig_Event();
        }

        private async Task AddEmailConfig_Event()
        {
            var emailConfigRequest = new EmailConfig();
            emailConfigRequest.ServiceName = TxtServiceName.Text;
            emailConfigRequest.Host = TxtHost.Text;
            emailConfigRequest.Port = int.Parse(TxtPort.Text);

            using var httpClient = new HttpClient();
            string url = "http://localhost:8081/comms/email/config/add";

            var json = JsonSerializer.Serialize(emailConfigRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, content);

            if (response.StatusCode != System.Net.HttpStatusCode.Created) {
                MessageBox.Show("An error has occurred when trying to create a new email config", "Error",  MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                this.Close();
            }

        }

        private void CancelAddEmailConfig_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
