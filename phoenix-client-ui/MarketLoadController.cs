using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.WebRequestMethods;

namespace phoenix_prototype
{
    public class MarketLoadController
    {

        public Dictionary<string, string> regionMap;
        
        public MarketLoadController() {
            regionMap = new Dictionary<string, string>();
            loadRegionMap(regionMap);
        }

        private void loadRegionMap(Dictionary<string, string> regionMap)
        {
            if (regionMap != null)
            {
                regionMap.Clear();
                regionMap.Add("all", "0");
                regionMap.Add("london", "1");
            }
        }

        

        public bool LoadMarketDataByRegion(string region)
        {
            string regionId = this.regionMap[region];
            if (regionId != null)
            {
                submitDataLoad(regionId);
            }
            return true;
        }


        private async Task submitDataLoad(string regionId)
        {
            // make the call to the client service to load the region
            string url = "http://localhost:8081/market/load/region?id=" + regionId;
            using var client = new HttpClient();
            var response = await client.PostAsync(url, null);
            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                MessageBox.Show("An error occurred when requesting the data load: \n" + response.Content, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

    }
}
