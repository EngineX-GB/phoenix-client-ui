using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace phoenix_prototype
{
    public class WatchlistEntry
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
       
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("nationality")]
        public string Nationality { get; set; }
        
        [JsonPropertyName("telephone")]
        public string Telephone { get; set; }
        
        [JsonPropertyName("rate")]
        public int Rate { get; set; }
        
        [JsonPropertyName("location")]
        public string Location { get; set; }
    }
}
