using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for search.xaml
    /// </summary>
    public partial class search : Window
    {
        private RatingsLoadController ratingsLoadController = new RatingsLoadController();

        private WindowConfig windowConfig { get; set; } = new WindowConfig();

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

        //public ObservableCollection<SearchEntry> searchEntries { get; set; } = new ObservableCollection<SearchEntry>();

        public search(AppDataService data)
        {
            InitializeComponent();
            _data = data;
            DataContext = _data;
        }

        // Make the REST call when the window opens
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAllOrdersAsync();
        }


        public async Task LoadAllOrdersAsync()
        {
            StatusText.Text = "Querying data...";

            try
            {
                using var client = new HttpClient();

                // Replace with your real API endpoint
                string url = "http://localhost:8081/search?nationality=British";

                var response = await client.GetAsync(url);
                Console.WriteLine(response.Content);

                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();

                var items = JsonSerializer.Deserialize<List<SearchEntry>>(json);

                _data.SearchEntries.Clear();
                foreach (var item in items)
                    _data.SearchEntries.Add(item);
                StatusText.Text = "Received " + _data.SearchEntries.Count + " client(s)";
            }
            catch (HttpRequestException hre)
            {
                Console.WriteLine(hre.Message);
                StatusText.Text = "Error: Unable to connect to the Client Service";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                StatusText.Text = "Error: An unexpected error has occurred. " + ex.Message;
            }
        }

        public async Task RunSearchQueryAsync()
        {
            var parameters = new List<string>();

            if (!string.IsNullOrWhiteSpace(Username.Text))
                parameters.Add($"username={Username.Text}");

            if (!string.IsNullOrWhiteSpace(UserID.Text))
                parameters.Add($"userId={UserID.Text}");

            if (!string.IsNullOrWhiteSpace(Nationality.Text))
                parameters.Add($"nationality={Nationality.Text}");

            if (!string.IsNullOrWhiteSpace(Region.Text))
                parameters.Add($"region={Region.Text}");

            string queryParams = string.Join("&", parameters);

            StatusText.Text = "Querying data...";

            try
            {

                using var client = new HttpClient();

                string url = "http://localhost:8081/search?" + queryParams;

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                var items = JsonSerializer.Deserialize<List<SearchEntry>>(json);

                _data.SearchEntries.Clear();
                foreach (var item in items)
                    _data.SearchEntries.Add(item);
                StatusText.Text = "Received " + _data.SearchEntries.Count + " client(s)";
            }
            catch (HttpRequestException hre)
            {
                Console.WriteLine(hre.Message);
                StatusText.Text = "Error: Unable to connect to the Client Service";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                StatusText.Text = "Error: An unexpected error has occurred. " + ex.Message;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) { this.Close(); }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (!windowConfig._lockEnabled)
                    this.DragMove();

            }
        }

        /* Program the refresh button*/
        private async void RunSearchQuery_Click(object sender, RoutedEventArgs e) {
            await RunSearchQueryAsync();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Ready";
            Username.Text = "";
            UserID.Text = "";      // This is your UserID field
            Nationality.Text = "";
            Region.Text = "";
            Keyboard.ClearFocus();
        }

        private async void SearchField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await RunSearchQueryAsync();
            }
        }

        private void DataGridSearchEntries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridSearch.SelectedItem is SearchEntry selectedSearchEntry)
            {
                _data.NotifySearchEntrySelected(selectedSearchEntry);
            }
        }

        private async void AddToWatchlist_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridSearch.SelectedItem is SearchEntry selected)
            {
                try
                {
                    StatusText.Text = "Adding...";

                    await _data.AddToWatchlistAsync(selected.UserId);

                    StatusText.Text = "Added to Watchlist";

                    // Notify Watchlist window to reload
                    _data.NotifyWatchlistUpdated();
                }
                catch (Exception ex)
                {
                    StatusText.Text = "Error adding to watchlist";
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                StatusText.Text = "No row selected";
            }
        }

        private void RaiseOrder_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridSearch.SelectedItem is SearchEntry selectedSearchEntry)
            {
                var form = new OrderRequestForm(selectedSearchEntry);
                form.ShowDialog();
            }

        }

        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            IsClosed = true;
            base.OnClosed(e);
        }


        private void FetchRatings_Click(object sender, RoutedEventArgs e)
        {
            // get the number of selected rows, extract the IDs, then create a request to the client service to fetch
            // up to date ratings data


            List<string> userIdList = new List<string>();
            var selectedUsers = DataGridSearch.SelectedItems.Cast<SearchEntry>().ToList();
            foreach (var user in selectedUsers)
            {
                userIdList.Add(user.UserId);
            }

            if (userIdList.Count > 0)
            {
                ratingsLoadController.LoadFeedbackData(userIdList);
            }

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
