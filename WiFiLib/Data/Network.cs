using System.Collections.Generic;
using Newtonsoft.Json;

namespace WiFiLib.Data
{
    public class Network
    {
        [JsonProperty("ssid")]
        public string SSID { get; protected set; }

        [JsonProperty("access_points")]
        public List<AccessPoint> AccessPoints { get; protected set; } = new List<AccessPoint>();
    }
}