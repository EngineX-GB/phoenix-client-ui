using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace phoenix_prototype
{


    public class SearchEntry : INotifyPropertyChanged
    {


        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly System.Timers.Timer BlinkTimer;
        private static bool BlinkState = false;

        static SearchEntry()
        {
            BlinkTimer = new System.Timers.Timer(3000); // 10 seconds
            BlinkTimer.Elapsed += (s, e) =>
            {
                BlinkState = !BlinkState;
                BlinkStateChanged?.Invoke();
            };
            BlinkTimer.Start();
        }

        private static event Action BlinkStateChanged;

        public SearchEntry()
        {
            BlinkStateChanged += () =>
            {
                OnPropertyChanged(nameof(R100Display));
                OnPropertyChanged(nameof(R100Color));
            };

        }

        protected void OnPropertyChanged(string prop)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            });
        }



        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("nationality")]
        public string Nationality { get; set; }

        [JsonPropertyName("ethnicity")]
        public string Ethnicity { get; set; }

        [JsonPropertyName("age")]
        public int Age { get; set; }
        [JsonPropertyName("rating")]
        public int Rating  { get; set; }
        [JsonPropertyName("telephone")]
        public string Telephone { get; set; }
        [JsonPropertyName("location")]
        public string Location { get; set; }
        [JsonPropertyName("r100")]
        public int R100 { get; set; }
        [JsonPropertyName("d100")]
        public int D100 { get; set; }
        [JsonPropertyName("r150")]
        public int R150 { get; set; }
        [JsonPropertyName("r200")]
        public int R200 { get; set; }

        [JsonPropertyName("minimumCharge")]
        public int MinimumCharge { get; set; }


        [JsonPropertyName("maximumCharge")]
        public int MaximumCharge { get; set; }
        
        [JsonPropertyName("numberOfDaysInService")]
        public int NumberOfDaysInService { get; set; }
        
        [JsonPropertyName("percentageAvailable")]
        public int PercentageAvailable { get; set; }
        
        [JsonPropertyName("totalRegionsTravelled")]
        public int TotalRegionsTravelled { get; set; }
        
        [JsonPropertyName("previouslyServicedBB")]
        public bool PreviouslyServicedBB { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("height")]
        public NullableDouble HeightData { get; set; }

        public double Height
        {
            get
            {
                if (HeightData == null || !HeightData.Valid) {
                    return 0.0;
                }
                return HeightData.Value;
            }
        }


        // Computed property used by the DataGrid
        public int R100Display => (D100 != 0 && BlinkState) ? D100 : R100;

        // Colour logic for both R100 and D100
        [JsonIgnore]
        public Brush R100Color
        {
            get
            {
                if (D100 < 0)
                    return Brushes.LimeGreen;   // negative → green

                if (D100 > 0)
                    return Brushes.Red;         // positive → red

                return Brushes.White;           // zero → normal
            }
        }


    }
}
