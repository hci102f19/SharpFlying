using Newtonsoft.Json;

namespace WiFiLib.Data
{
    public class AccessPoint
    {
        [JsonProperty("quality")]
        public string Quality { get; protected set; }

        [JsonProperty("mac")]
        public string Mac { get; protected set; }

        [JsonProperty("essid")]
        public string ESSID { get; protected set; }

        [JsonProperty("signal")]
        public string Signal { get; protected set; }
    }
}