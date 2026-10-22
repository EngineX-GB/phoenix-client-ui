using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace phoenix_prototype
{
    public class NullableDouble
    {

        [JsonPropertyName("Float64")]
        public double Value { get; set; }

        [JsonPropertyName("Valid")]
        public bool Valid { get; set; }


    }
}
