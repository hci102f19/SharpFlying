using System;
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

        [JsonProperty("frequency")]
        public int Frequency { get; protected set; }

        [JsonProperty("distance")]
        public double Distance { get; protected set; }

        public double Radius()
        {
            return Math.Round(Math.Pow(10, (27.55 - (20 * Math.Log10(Frequency)) + Math.Abs(Signal)) / 20), 2);
        }

        public double Area()
        {
            return Math.PI * Radius() * 2;
        }
    }
}