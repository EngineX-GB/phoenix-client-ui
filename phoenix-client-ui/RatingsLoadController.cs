using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace phoenix_prototype
{
    public class RatingsLoadController
    {
        public RatingsLoadController() { }

        public bool LoadFeedbackData(List<string> userIdList)
        {
            submitDataLoad(userIdList);
            return true;
        }


        private async Task submitDataLoad(List<string> userIdList)
        {
            // make the call to the client service to load the region
            string url = "http://localhost:8081/feedback/load";
            using var client = new HttpClient();


            var json = JsonSerializer.Serialize(userIdList);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                MessageBox.Show("An error occurred when requesting the data load: \n" + response.Content, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

    }
}
