using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace phoenix_prototype
{
    public class ServiceReportHeadlineEntry
    {
        [JsonPropertyName("oid")]
        public long Oid { get; set; }
        
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
        
        [JsonPropertyName("meetDate")]
        public DateTime MeetDate { get; set; }

        [JsonPropertyName("reportRating")]
        public string ReportRating { get; set; }

        [JsonPropertyName("headline")]
        public string Headline { get; set; }

    }
}
