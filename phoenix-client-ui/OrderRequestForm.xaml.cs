using System;
using System.ComponentModel;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace phoenix_prototype
{
    public partial class OrderRequestForm : PhoenixThemedWindow, INotifyPropertyChanged
    {
        private SearchEntry searchEntry;

        private string orderMode = "PROPOSED";

        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private int selectedRate; // the marketPrice for a given quantity/ duration. Not including surplus and deduction.

        private int _marketPrice;
        public int MarketPrice
        {
            get => _marketPrice;
            set
            {
                _marketPrice = value;
                Notify(nameof(MarketPrice));
            }
        }

        public OrderRequestForm(SearchEntry searchEntry)
        {
            InitializeComponent();
            DataContext = this;   // REQUIRED for binding to work
            this.searchEntry = searchEntry;
            PopulateOrderDetails(searchEntry);
        }

        private void ProposedOrderMode_Click(object sender, RoutedEventArgs e)
        {
            ProposedMode.Background = Brushes.ForestGreen;
            CommittedMode.Background = (Brush)new BrushConverter().ConvertFromString("#333");
            this.orderMode = "PROPOSED";
        }

        private void CommittedOrderMode_Click(object sender, RoutedEventArgs e)
        {
            CommittedMode.Background = Brushes.ForestGreen;
            ProposedMode.Background = (Brush)new BrushConverter().ConvertFromString("#333");
            this.orderMode = "COMMITTED";
        }

        private void PopulateOrderDetails(SearchEntry searchEntry)
        {
            if (searchEntry == null)
            {
                MessageBox.Show("Select an asset first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TxtUserID.Text = searchEntry.UserId;
            TxtUsername.Text = searchEntry.Username;
            TxtLocation.Text = searchEntry.Location;

            TxtTradeDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            TxtTradeTime.Text = "00:00:00";
        }

        private void AdjustQuantityAndPrice(object sender, RoutedEventArgs e)
        {
            if (searchEntry == null)
                return;

            string qty = TxtQuantity.Text?.Trim();

            if (qty == "1")
            {
                if (searchEntry.R100 > 0)
                {
                    MarketPrice = IncludeSurplusAndDeductions(TxtSurplus.Text, TxtDeductions.Text, searchEntry.R100);
                    selectedRate = searchEntry.R100;
                }
            }
            else if (qty == "1.5")
            {
                if (searchEntry.R150 > 0)
                {
                    MarketPrice = IncludeSurplusAndDeductions(TxtSurplus.Text, TxtDeductions.Text, searchEntry.R150);
                    selectedRate = searchEntry.R150;
                }
            }
            else if (qty == "2")
            {
                if (searchEntry.R200 > 0)
                {
                    MarketPrice = IncludeSurplusAndDeductions(TxtSurplus.Text, TxtDeductions.Text, searchEntry.R200);
                    selectedRate = searchEntry.R200;
                }
            }
            else
            {
                MarketPrice = 0;
            }
        }

        private int IncludeSurplusAndDeductions(string surplusText, string deductionsText, int marketPrice)
        {
            int surplus = surplusText != null && !surplusText.Equals("") ? int.Parse(surplusText) : 0;
            int deductions = deductionsText != null && !deductionsText.Equals("") ? int.Parse(deductionsText) : 0;
            return marketPrice + surplus - deductions;
        }

        private bool validateOrderForm()
        {
            return !TxtUserID.Text.Equals("") && !TxtUsername.Text.Equals("")
                && !TxtLocation.Text.Equals("")
                && !TxtQuantity.Text.Equals("")
                && !TxtTradeDate.Text.Equals("")
                && !TxtTradeTime.Text.Equals("");
        }


        private async Task submitOrder()
        {

            int surplus = !TxtSurplus.Text.Equals("") ? int.Parse(TxtSurplus.Text) : 0;
            int deductions = !TxtDeductions.Text.Equals("") ? int.Parse(TxtDeductions.Text) : 0;
            string defaultNotesString = !TxtNotes.Text.Equals("") ? TxtNotes.Text : "Order submitted on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


            string url = "http://localhost:8081/orders";
            using var httpClient = new HttpClient();
            // prepare request

            OrderEntry order = new OrderEntry();
            order.UserId = TxtUserID.Text;
            order.Username = TxtUsername.Text;
            order.Location = TxtLocation.Text;
            order.Duration = int.Parse(TxtQuantity.Text);
            order.Region = this.searchEntry.Region;
            order.DateOfEvent = DateTime.ParseExact(TxtTradeDate.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            order.TimeOfEvent = TimeSpan.ParseExact(TxtTradeTime.Text, "hh\\:mm\\:ss", CultureInfo.InvariantCulture);
            order.Rate = selectedRate;
            order.Surplus = surplus;
            order.Deductions = deductions;
            order.Price = MarketPrice;
            order.Status = this.orderMode;
            order.Notes = defaultNotesString;

            var json = JsonSerializer.Serialize(order);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, content);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                MessageBox.Show("An error occurred when trying to save the order: \n" + response.Content , "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                this.Close();
            }

        }

        private async void SubmitOrderRequest(object sender, RoutedEventArgs e)
        {
            if (validateOrderForm())
            {
                // send the form
                await submitOrder();

            } else
            {
                MessageBox.Show("Unable to save order due to invalid field parameters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}