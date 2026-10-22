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

    public partial class EmailAccount
    {
        [JsonPropertyName("oid")]
        public int Oid { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("formalName")]
        public string FormalName { get; set; }

        [JsonPropertyName("emailConfigId")]
        public int EmailConfigId { get; set; }

    }





    /// <summary>
    /// Interaction logic for EmailAccountDialog.xaml
    /// </summary>
    public partial class EmailAccountDialog : Window
    {
        public ObservableCollection<EmailAccount> EmailAccounts { get; set; } = new ObservableCollection<EmailAccount>();

        public EmailAccountDialog()
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

        private async void AddEmailAccount_Click(object sender, RoutedEventArgs e)
        {
            var newEmailAccountDialog = new NewEmailAccount();
            newEmailAccountDialog.Owner = this;
            newEmailAccountDialog.ShowDialog();
            GetSavedEmailAccountData();
        }

        private void DeleteEmailAccount_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Are you sure you want to delete this account?", "Delete Account", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                // run the logic to delete the config

                if (DataGridEmailAccounts.SelectedItem is EmailAccount selected)
                {
                    DeleteEmailAccountData(selected.Oid);
                }
            }
        }


        private async Task DeleteEmailAccountData(int userId)
        {
            using var httpClient = new HttpClient();
            string url = "http://localhost:8081/comms/email/account/remove?id=" + userId;
            var response = await httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show("An error occured when trying to delete the account", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            else
            {
                GetSavedEmailAccountData();
            }
        }

        private async Task GetSavedEmailAccountData()
        {
            using var httpClient = new HttpClient();
            string emailConfigUrl = "http://localhost:8081/comms/email/accounts";
            var response = await httpClient.GetAsync(emailConfigUrl);
            response.EnsureSuccessStatusCode();
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show("An error occurred when retrieving saved email account", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                string json = await response.Content.ReadAsStringAsync();
                var items = JsonSerializer.Deserialize<List<EmailAccount>>(json);

                EmailAccounts.Clear();
                foreach (var item in items)
                    EmailAccounts.Add(item);
            }
        }

        private async void LoadOnStartup(object sender, RoutedEventArgs e)
        {
            await GetSavedEmailAccountData();
        }











    }
}
