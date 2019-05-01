using Newtonsoft.Json;

namespace WiFiLib.Data
{
    public class AccessPoint
    {
        [JsonProperty("mac")]
        public string Mac { get; protected set; }

        [JsonProperty("essid")]
        public string ESSID { get; protected set; }

        [JsonProperty("quality")]
        public double Quality { get; protected set; }

        [JsonProperty("signal")]
        public double Signal { get; protected set; }

        [JsonProperty("distance")]
        public double Distance { get; protected set; }

    }
}