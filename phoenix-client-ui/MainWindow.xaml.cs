using Microsoft.Win32;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;

namespace phoenix_prototype
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Window windowWatchlist;
        private Window windowOrders;
        private Window windowNews;
        private Window windowSearch;
        private Notifications windowNotifications;

        private bool _isDraggingTitleBar = false;
        private Point _dragOffset; // not used
        private Dictionary<string, Point> offsetPointDict = new Dictionary<string, Point>();
        public List<Window> AttachedWindows = new();

        // market controller for loading regions
        private MarketLoadController marketLoadController = new MarketLoadController();

        private Rect _restoreBounds;

        private bool _lockAttachedWindows;

       
        private readonly MainViewModel _vm;

        public AppDataService DataService { get; } = new();

        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel(this);
            DataContext = _vm;

            this.LocationChanged += MainWindow_LocationChanged;

            Debug.WriteLine("MainWindow DataContext set to MainViewModel");
            Loaded += async (_, __) => await _vm.InitializeAsync();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var workArea = SystemParameters.WorkArea; 
            this.Left = workArea.Left; 
            this.Top = workArea.Top; 
            this.Width = workArea.Width; 
            this.Height = workArea.Height;


        }

        public void AddNotification(NotificationListEntry entry)
        {
            windowNotifications?.AddNotificationToList(entry);

            // TODO: 2026-08-30: Could this be the spot where you add the notification entry to the search listings
            // screen to highlight rows that have feedback downloaded?
        }



        private void WindowAdjustTestButton_Click(object sender, RoutedEventArgs e)
        {
            setHardDimensionsOnWindow(windowWatchlist, 425.6, 396, -0.8, 76);
            setHardDimensionsOnWindow(windowOrders, 326.40000000000003, 397.6, 0, 498.40000000000003);
            setHardDimensionsOnWindow(windowSearch, 450.40000000000003, 627.2, 388.8, 73.60000000000001);
            setHardDimensionsOnWindow(windowNews, 305.6, 627.2, 389.6, 520);
        }

        private void WindowStatsButton_Click(object sender, RoutedEventArgs e)
        {

            Debug.WriteLine("Full Screen Width: " + SystemParameters.PrimaryScreenWidth);
            Debug.WriteLine("Full Screen Height: " + SystemParameters.PrimaryScreenHeight);


            // work area excluding the task bar:

            Rect workArea = SystemParameters.WorkArea;

            double usableWidth = workArea.Width;
            double usableHeight = workArea.Height;
            double usableLeft = workArea.Left;
            double usableTop = workArea.Top;

            readWindowStats(windowSearch, "SEARCH");
            readWindowStats(windowOrders, "ORDERS");
            readWindowStats(windowNews, "NEWS");
            readWindowStats(windowWatchlist, "WATCHLIST");

        }



        private void readWindowStats (Window window, string windowName)
        {
            if (window != null)
            {
                Debug.WriteLine("DEBUG>> [" + windowName + "_WINDOW] Height is " + window.Height);
                Debug.WriteLine("DEBUG>> [" + windowName+ "] Width is " + window.Width);
                Debug.WriteLine("DEBUG>> [" + windowName + "] X point coordinate: " + window.Left);
                Debug.WriteLine("DEBUG>> [" + windowName+  "] Y point coordinate: " + window.Top);

            }
        }

        private void setHardDimensionsOnWindow(Window window, double height, double width, double pointX, double pointY)
        {
            if (window != null)
            {
                window.Height = height;
                window.Width = width;
                window.Left = pointX;
                window.Top = pointY;
            }
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e) { this.Close(); }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadMarketData_LondonRegion_Click(object sender, RoutedEventArgs e)
        {
            if (this.marketLoadController != null) {
                marketLoadController.LoadMarketDataByRegion("london");
            }
        }

        private void LoadMarketData_AllRegions_Click(object sender, RoutedEventArgs args)
        {
            if (this.marketLoadController != null)
            {
                marketLoadController.LoadMarketDataByRegion("all");
            }
        }


        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed)
            {

                // create a dictionary containing the window names and the offsets



                // Capture relative offsets for each attached window
                foreach (var win in AttachedWindows)
                {
                    Debug.WriteLine("[win] " + win.Name + "left : " + win.Left + ", top : " + win.Top);
                    var dragOffset = new Point(win.Left - this.Left, win.Top - this.Top);
                    offsetPointDict[win.Name] = dragOffset;
                }


                _isDraggingTitleBar = true;

                try
                {
                    DragMove();   // This blocks until the drag ends
                }
                finally
                {
                    _isDraggingTitleBar = false;
                }
            }
        }


        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            if (!_isDraggingTitleBar)
                return;

            foreach (var win in AttachedWindows)
            {
                win.Left = this.Left + offsetPointDict[win.Name].X;
               win.Top = this.Top + offsetPointDict[win.Name].Y;
                Debug.WriteLine(win.Name + ", " + win.Left + ", "+win.Top );
                Debug.WriteLine("Main window : " + this.Left + ", " + this.Top);
            }
        }




        private void Watchlist_Click(object sender, RoutedEventArgs e)
        {
            windowWatchlist = new Watchlist(DataService);   // Watchlist.xaml → class Watchlist
            windowWatchlist.Owner = this;            // Optional: keeps it tied to main window
            this.AttachedWindows.Add(windowWatchlist);
            windowWatchlist.Show();                  // or ShowDialog() if you want modal
        }

        private void Notifications_Click(object sender, RoutedEventArgs e)
        {
            windowNotifications = new Notifications();
            this.AttachedWindows.Add(windowNotifications);
            windowNotifications.Owner = this;
            windowNotifications.Show();
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            windowOrders = new Orders(DataService);
            this.AttachedWindows.Add(windowOrders);
            windowOrders.Owner = this;
            windowOrders.Show();
        }

        private void News_Click(object sender, RoutedEventArgs e)
        {
            windowNews = new Reports(DataService);
            this.AttachedWindows.Add(windowNews);
            windowNews.Owner = this;
            windowNews.Show();
        }

        private void Listings_Click(object sender, RoutedEventArgs args)
        {
            windowSearch = new search(DataService);
            windowSearch.Owner = this;
            windowSearch.Show();
        }


        private void MinimiseButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }


        private void MaximiseButton_Click(object sender, RoutedEventArgs e)
        {
            // Save restore bounds
            _restoreBounds = new Rect(Left, Top, Width, Height);

            var wa = SystemParameters.WorkArea;

            WindowState = WindowState.Normal; // important
            Left = wa.Left;
            Top = wa.Top;
            Width = wa.Width;
            Height = wa.Height;


            foreach (var win in AttachedWindows)
            {
                win.Left = this.Left + offsetPointDict[win.Name].X;
                win.Top = this.Top + offsetPointDict[win.Name].Y;
                Debug.WriteLine(win.Name + ", " + win.Left + ", " + win.Top);
                Debug.WriteLine("Main window : " + this.Left + ", " + this.Top);
            }
        }



        public async void IngestionPingButton_Click(object sender, RoutedEventArgs e)
        {
            await TogglePingService("ingestion");
        }


        public async void MarketPingButton_Click(object sender, RoutedEventArgs e)
        {
            await TogglePingService("market");
        }

        public void SmtpSettings_Click(object sender, RoutedEventArgs e)
        {
            var emailConfigDialog = new EmailConfigDialog();
            emailConfigDialog.Owner = this;
            emailConfigDialog.ShowDialog();
        }

        public void EmailAccountSettings_Click(object sender, RoutedEventArgs e)
        {
            var emailAccountDialog = new EmailAccountDialog();
            emailAccountDialog.Owner = this;
            emailAccountDialog.ShowDialog();
        }


        public void MarketLoadRegionDialog_Click(object sender, RoutedEventArgs e)
        {
            var marketLoadRegionDialog = new MarketRegionLoadDialog(this.marketLoadController);
            marketLoadRegionDialog.Owner = this;
            marketLoadRegionDialog.ShowDialog();
        }

        public void ImportLayoutDialog_Click(object sender, RoutedEventArgs e)
        {
            var importLayoutFileDialog = new OpenFileDialog
            {
                Title = "Import Layout",
                Filter = "Json Files (*.json)|*.json",
                DefaultExt = "json"
            };

            bool? result = importLayoutFileDialog.ShowDialog();
            if (result == true )
            {
                string filePath = importLayoutFileDialog.FileName;
                List<WindowMetadata> listOfWindowMetadata = ViewManager.ImportLayoutFromFile(filePath);
                if (listOfWindowMetadata.Count == 0)
                {
                    MessageBox.Show("Unable to import the layout data", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // clear out the existing windows in the view:

                this.AttachedWindows.Clear();

                // TODO: clear down all the windows (dispose of them)


                // go through the windows metadata list and recreate the windows.

                foreach (var winMetadata in listOfWindowMetadata)
                {
                    // load the window with the specified position and dimensions.
                    addAttachedWindow(winMetadata);
                }

            }
        }


        private void addAttachedWindow(WindowMetadata windowMetadata)
        {
            if (windowMetadata.windowsName == "_Search")
            {
                if (windowSearch == null)
                {
                    windowSearch = new search(DataService);
                    windowSearch.Owner = this;
                }

                if (windowSearch != null && windowSearch is search search)
                {
                    if (search.IsClosed)
                    {
                        windowSearch = new search(DataService);
                        windowSearch.Owner = this;
                    }
                }
                
                windowSearch.Top = windowMetadata.top;
                windowSearch.Left = windowMetadata.left;
                windowSearch.Height = windowMetadata.height;
                windowSearch.Width = windowMetadata.width;
                this.AttachedWindows.Add(windowSearch);
                windowSearch.Show();
            }
            if (windowMetadata.windowsName == "_Watchlist")
            {
                if (windowWatchlist == null)
                {
                    windowWatchlist = new Watchlist(DataService);
                    windowWatchlist.Owner = this;
                }

                if (windowWatchlist != null && windowWatchlist is Watchlist watchlist)
                {
                    if (watchlist.IsClosed)
                    {
                        windowWatchlist = new Watchlist(DataService);
                        windowWatchlist.Owner = this;
                    }
                }

                windowWatchlist.Top = windowMetadata.top;
                windowWatchlist.Left = windowMetadata.left;
                windowWatchlist.Height = windowMetadata.height;
                windowWatchlist.Width = windowMetadata.width;
                this.AttachedWindows.Add(windowWatchlist);
                windowWatchlist.Show();
            }
            if (windowMetadata.windowsName == "_Notifications")
            {
                if (windowNotifications == null)
                {
                    windowNotifications = new Notifications();
                    windowNotifications.Owner = this;
                }
                if (windowNotifications != null && windowNotifications is Notifications notifications)
                {
                    if (notifications.IsClosed)
                    {
                        windowNotifications = new Notifications();
                        windowNotifications.Owner = this;
                    }
                }
                windowNotifications.Top = windowMetadata.top;
                windowNotifications.Left = windowMetadata.left;
                windowNotifications.Height = windowMetadata.height;
                windowNotifications.Width = windowMetadata.width;
                this.AttachedWindows.Add(windowNotifications);
                windowNotifications.Show();
            }
            if (windowMetadata.windowsName == "_News")
            {
                if (windowNews == null) 
                {
                    windowNews = new Reports(DataService);
                    windowNews.Owner = this;
                }
                if (windowNews != null && windowNews is Reports reports)
                {
                    if (reports.IsClosed)
                    {
                        windowNews = new Reports(DataService);
                        windowNews.Owner = this;
                    }
                }

                this.AttachedWindows.Add(windowNews);
                windowNews.Top = windowMetadata.top;
                windowNews.Left = windowMetadata.left;
                windowNews.Height = windowMetadata.height;
                windowNews.Width = windowMetadata.width;
                windowNews.Show();
            }
            if (windowMetadata.windowsName == "_Orders")
            {
                if (windowOrders == null)
                {
                    windowOrders = new Orders(DataService);
                    windowOrders.Owner = this;
                }
                if (windowOrders != null && windowOrders is Orders orders)
                {
                    if (orders.IsClosed)
                    {
                        windowOrders = new Orders(DataService);
                        windowOrders.Owner = this;
                    }
                }

                this.AttachedWindows.Add(windowOrders);
                windowOrders.Top = windowMetadata.top;
                windowOrders.Left = windowMetadata.left;
                windowOrders.Height = windowMetadata.height;
                windowOrders.Width = windowMetadata.width;
                windowOrders.Show();
            }

        }



        public void ExportLayoutDialog_Click(object sender, RoutedEventArgs e)
        {
            var exportLayoutFileDialog = new SaveFileDialog
            {
                Title = "Export Layout File",
                Filter = "Json Files (*.json)|*.json",
                DefaultExt = "json",
                FileName = "CustomLayout"
            };

            bool? result = exportLayoutFileDialog.ShowDialog();

            if (result == true)
            {
                string filePath = exportLayoutFileDialog.FileName;

                bool exporResult = ViewManager.ExportLayoutToFile(AttachedWindows, filePath);
                if (!exporResult) {
                    MessageBox.Show("An error occurred when trying to export the layoout", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // button to toggle the ingestion service pinging

        public async Task TogglePingService(string serviceName)
        {
            using var client = new HttpClient();

            // Replace with your real API endpoint
            string url = "http://localhost:8081/monitor/ping?serviceName=" + serviceName;

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
        }

        
        // for double clicking on the custom titlebar and maximing the window
        private void TitleBar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var window = Window.GetWindow((DependencyObject)sender);


                if (window.WindowState == WindowState.Normal)
                {
                    CustomMaximisedWindow();
                }
                else
                {
                    window.WindowState = WindowState.Normal;
                }
            }
        }

        /**
         * This is a function to work around maximising the window to use the full work area (not including the task bar)
         * rather than using WindowState.MAXIMIZED which leaves a gap on the bottom and right side of the window.
         */
        private void CustomMaximisedWindow()
        {
            // Save restore bounds
            _restoreBounds = new Rect(Left, Top, Width, Height);

            var wa = SystemParameters.WorkArea;

            WindowState = WindowState.Normal; // important
            Left = wa.Left;
            Top = wa.Top;
            Width = wa.Width;
            Height = wa.Height;


            foreach (var win in AttachedWindows)
            {
                win.Left = this.Left + offsetPointDict[win.Name].X;
                win.Top = this.Top + offsetPointDict[win.Name].Y;
                Debug.WriteLine(win.Name + ", " + win.Left + ", " + win.Top);
                Debug.WriteLine("Main window : " + this.Left + ", " + this.Top);
            }
        }

        private void LoadDefaultLayout_Click(object sender, RoutedEventArgs e)
        {
            LoadDefaultLayout();
        }

        private void LoadDefaultLayout()
        {
            this.AttachedWindows.Clear();
            List<WindowMetadata> defaultWindowMetadata = ViewManager.PrepareDefaultLayout();
            if (defaultWindowMetadata.Count > 0 )
            {
                foreach (var win in defaultWindowMetadata)
                {
                    addAttachedWindow(win);
                }
            }
        }


    }
}