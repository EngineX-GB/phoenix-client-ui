using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;


namespace phoenix_prototype
{
    /// <summary>
    /// Interaction logic for Reports.xaml
    /// </summary>
    public partial class Reports : Window
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

        private WindowConfig windowConfig { get; set; } = new WindowConfig();

        public Reports(AppDataService data)
        {
            InitializeComponent();
            _data = data;
            DataContext = _data;
            _data.SearchEntrySelected += OnSearchRowSelected;   // subscribe to the event when a search entry is selected, then News panel should react
        }

        /**
         * When a row is selected on the search grid (i.e when the asset is selected, then show the ratings/ news
         * for that asset, taking the user id field and fetching the news on the News Panel.
         */
        private async void OnSearchRowSelected(SearchEntry searchEntry)
        {
            if (searchEntry.UserId != "")
            {
                UserID.Text = searchEntry.UserId;
                await LoadFeedbackAsync(searchEntry.UserId, null);
                await LoadServiceReportsAsync(UserID.Text);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {}

        private void CloseButton_Click(object sender, RoutedEventArgs e) { this.Close(); }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (!windowConfig._lockEnabled)
                    this.DragMove();

            }
        }

        /* Program the search button*/
        private async void SearchButton_Click(object sender, RoutedEventArgs e) { 
            if (UserID.Text != "")
            {
                await LoadFeedbackAsync(UserID.Text, null);
                await LoadServiceReportsAsync(UserID.Text);
            }
        }

        private async void SearchField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (UserID.Text != "")
                {
                    await LoadFeedbackAsync(UserID.Text, null);
                }
            }
        }

        public async void NavigateNextPageResults_Click(object sender, RoutedEventArgs e)
        {
            await LoadFeedbackAsync(UserID.Text, "FORWARD");
        }

        public async void NavigateToFirstPageResults_Click(object sender, RoutedEventArgs e)
        {
            await LoadFeedbackAsync(UserID.Text, null);
        }

        /**
         * TODO: 2026-03-07: This needs to be further investigated as it doesn't appear to work correctly.
         */
        public async void NavigatePreviousPageResults_Click(object sender, RoutedEventArgs e)
        {
            await LoadFeedbackAsync(UserID.Text, "BACKWARD");
        }


        public async Task LoadFeedbackAsync(string userId, string direction)
        {
            string url = null;
            using var client = new HttpClient();

            if (direction == null)
            {
                url = "http://localhost:8081/feedback?userId=" + userId;
            }
            else if (direction == "FORWARD")
            {
                var feedbackEntry = _data.FeedbackEntries[^1];    // get the last entry in the collection that is currently viewing (before getting the next batch)
                int offsetId = feedbackEntry.Oid;
                url = "http://localhost:8081/feedback?userId=" + userId + "&pageDirection=" + direction + "&offset=" + offsetId.ToString();
            }
            else
            {
                var feedbackEntry = _data.FeedbackEntries.First();
                int offsetId = feedbackEntry.Oid;
                url = "http://localhost:8081/feedback?userId=" + userId + "&pageDirection=" + direction + "&offset=" + offsetId.ToString();
            }

            var response = await client.GetAsync(url);
            Console.WriteLine(response.Content);

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            var items = JsonSerializer.Deserialize<FeedbackResponse>(json);

            _data.FeedbackEntries.Clear();
            foreach (var item in items.entries)
                _data.FeedbackEntries.Add(item);
        }

        public async Task LoadServiceReportsAsync(string userId)
        {
            string url = null;
            using var client = new HttpClient();
            url = "http://localhost:8081/servicereports?userId=" + userId;
            var response = await client.GetAsync(url);
            Console.WriteLine(response.Content);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<List<ServiceReportHeadlineEntry>>(json);

            _data.ServiceReportHeadlineEntries.Clear();
            foreach (var item in items)
                _data.ServiceReportHeadlineEntries.Add(item);
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
