using System;
using System.Collections.Generic;
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
    /// Interaction logic for MarketRegionLoadDialog.xaml
    /// </summary>
    public partial class MarketRegionLoadDialog : Window
    {

        private MarketLoadController marketLoadController;

        public MarketRegionLoadDialog(MarketLoadController marketLoadController)
        {
            InitializeComponent();
            this.marketLoadController = marketLoadController;

            RegionListView.SelectedIndex = 0;
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


        private void LoadRegion_Click(object sender, RoutedEventArgs e)
        {
            if (marketLoadController != null)
            {
                // get the selected item
                string selectedRegion = "all"; // default to all (if the option is not registered in the app)

                if (RegionListView.SelectedItem is ListViewItem selectedItem)
                {
                    selectedRegion = selectedItem.Content.ToString();

                   // MessageBox.Show($"Selected region: {selectedRegion}");
                }


                marketLoadController.LoadMarketDataByRegion(selectedRegion.ToLower());
                this.Close();
            }
        }


        private void Cancel_click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
