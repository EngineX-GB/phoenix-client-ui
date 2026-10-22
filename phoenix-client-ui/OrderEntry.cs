using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace phoenix_prototype
{
    public class OrderEntry : INotifyPropertyChanged
    {
        [JsonPropertyName("id")]
        public int Id {  get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; }
        
        [JsonPropertyName("username")]
        public string Username { get; set; }
        
        [JsonPropertyName("location")]
        public string Location { get; set; }
        
        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("rate")]
        public int Rate { get; set; }

        [JsonPropertyName("surplus")]
        public int Surplus { get; set; }

        [JsonPropertyName("deductions")]
        public int Deductions { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }

        
        [JsonPropertyName("dateOfEvent")]
        public DateTime DateOfEvent { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; }
        
        [JsonPropertyName("timeOfEvent")]
        public TimeSpan TimeOfEvent { get; set; }

        private string _status;

        [JsonPropertyName("status")]
        public string Status {

            get => _status; set { if (_status != value) { _status = value; OnPropertyChanged(nameof(Status)); } }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) { 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
        }

    }
}
