using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
    /// Interaction logic for Notifications.xaml
    /// </summary>
    public partial class Notifications : Window
    {

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


        public ObservableCollection<NotificationListEntry> NotificationListEntries { get; } = new ObservableCollection<NotificationListEntry>();

        private WindowConfig windowConfig { get; set; } = new WindowConfig();
        
        public Notifications()
        {
            InitializeComponent();
            DataContext = this;   // <-- IMPORTANT
        }

        public void AddNotificationToList(NotificationListEntry entry)
        {
            int MaxNotifications = 20;
            NotificationListEntries.Insert(0, entry);
            // here we don't want to keep adding endless notification messages. Instead limit it, such that
            // if the maximum capacity is met at 20, then add a new one to the top, but remove the old one from the bottom
            if (NotificationListEntries.Count > MaxNotifications)
                NotificationListEntries.RemoveAt(NotificationListEntries.Count - 1);
        }

        private void TestNotifications_Click(object sender, RoutedEventArgs e)
        {
            NotificationListEntry entry = new NotificationListEntry();
            entry.Timestamp = "00:00:00";
            entry.Status = "OK";
            entry.Detail = "Test Message";

            NotificationListEntries.Add(entry);

        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (!windowConfig._lockEnabled)
                    this.DragMove();
            
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) { this.Close(); }

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

        private void ClearNotifications_Click(object sender, RoutedEventArgs e) {
            NotificationListEntries.Clear();
        }

        private void PauseNotifications_Click(object sender, RoutedEventArgs e) { this.Close(); }

        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            IsClosed = true;
            base.OnClosed(e);
        }

    }
}
