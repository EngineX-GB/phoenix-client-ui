using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace phoenix_prototype
{
    public class Headers
    {
        [JsonPropertyName("min")]
        public long Min {  get; set; }

        [JsonPropertyName ("max")]
        public long Max { get; set; }
    }
}
