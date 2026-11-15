using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phoenix_prototype.config
{
    public class AppSettings
    {
        public string Environment { get; set; } = "PROD";

        public string ClientServiceEndpoint { get; set; } =
            "http://localhost:8081";
    }
}
