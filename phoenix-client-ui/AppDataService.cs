using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace phoenix_prototype
{
    public class AppDataService
    {

        public event Action<SearchEntry> SearchEntrySelected;

        public void NotifySearchEntrySelected(SearchEntry searchEntry)
        {
            SearchEntrySelected?.Invoke(searchEntry);
        }

        public event Func<Task>? WatchlistUpdated;

        public async void NotifyWatchlistUpdated()
        {
            if (WatchlistUpdated != null)
                await WatchlistUpdated.Invoke();
        }

        public async Task AddToWatchlistAsync(string UserId)
        {
            var payload = new
            {
                userId = UserId   // <-- must match your backend field name
            };


            using var client = new HttpClient();

            // POST the object as JSON
            var response = await client.PostAsJsonAsync(
                "http://localhost:8081/watchlist",
                payload
            );

            response.EnsureSuccessStatusCode();
        }

        public ObservableCollection<OrderEntry> OrderEntries { get; } = new();
        public ObservableCollection<WatchlistEntry> WatchlistCollection { get; } = new();

        public ObservableCollection<FeedbackEntry> FeedbackEntries { get; } = new();
        public ObservableCollection<ServiceReportHeadlineEntry> ServiceReportHeadlineEntries { get; } = new();

        public ObservableCollection<SearchEntry> SearchEntries { get; } = new();
    }

}
