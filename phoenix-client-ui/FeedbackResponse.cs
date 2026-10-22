using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace phoenix_prototype
{
    public class FeedbackResponse
    {
        [JsonPropertyName("headers")]
        public Headers headers {  get; set; }
        [JsonPropertyName("feedbackData")]
        public List<FeedbackEntry> entries { get; set; }
    }
}
