using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    public partial class EmailConfig
    {
        [JsonPropertyName("oid")]
        public int Oid { get; set; }

        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }

        [JsonPropertyName("name")]
        public string ServiceName { get; set; }
    }
    
    /// <summary>
    /// Interaction logic for EmailConfigDialog.xaml
    /// </summary>
    public partial class EmailConfigDialog : Window
    {

        public ObservableCollection<EmailConfig> EmailConfigs { get; set; } = new ObservableCollection<EmailConfig>();

        public EmailConfigDialog()
        {
            InitializeComponent();
            DataContext = this;
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

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) this.DragMove(); }

        private void CloseButton_Click(object sender, RoutedEventArgs e) { this.Close(); }

        private async void AddEmailConfig_Click(object sender, RoutedEventArgs e)
        {
            var newEmailConfigDialog = new NewEmailConfig();
            newEmailConfigDialog.Owner = this;
            newEmailConfigDialog.ShowDialog();
            GetSavedEmailConfigData();
        }

        private void DeleteEmailConfig_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Are you sure you want to delete this configuration?", "Delete Configuration", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                // run the logic to delete the config

                if (DataGridEmailConfigs.SelectedItem is EmailConfig selected)
                {
                    DeleteEmailConfigData(selected.Oid);
                }
            }
        }


        private async Task DeleteEmailConfigData(int userId)
        {
            using var httpClient = new HttpClient();
            string url = "http://localhost:8081/comms/email/config/remove?id=" + userId;
            var response = await httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
               MessageBox.Show("An error occured when trying to delete the config", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
            } else
            {
                GetSavedEmailConfigData();
            }
        }

        private async Task GetSavedEmailConfigData()
        {
            using var httpClient = new HttpClient();
            string emailConfigUrl = "http://localhost:8081/comms/email/configs";
            var response = await httpClient.GetAsync(emailConfigUrl);
            response.EnsureSuccessStatusCode();
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show("An error occurred when retrieving saved email config", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                string json = await response.Content.ReadAsStringAsync();
                var items = JsonSerializer.Deserialize<List<EmailConfig>>(json);

                EmailConfigs.Clear();
                foreach (var item in items)
                    EmailConfigs.Add(item);
            }
        }

        private async void LoadOnStartup(object sender, RoutedEventArgs e)
        {
            await GetSavedEmailConfigData();
        }

    }
}
