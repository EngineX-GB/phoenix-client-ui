using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace phoenix_prototype
{
    public class FeedbackEntry
    {
        public FeedbackEntry() { }

        [JsonPropertyName("oid")]
        public int Oid { get; set; }

        [JsonPropertyName("rating")]
        public string Rating { get; set; }

        [JsonPropertyName("ratingDate")]
        public DateTime RatingDate { get; set; }

        [JsonPropertyName("feedback")]
        public string Feedback {  get; set; }

        [JsonPropertyName("feedbackResponse")]
        public string FeedbackResponse { get; set; }

    }
}
