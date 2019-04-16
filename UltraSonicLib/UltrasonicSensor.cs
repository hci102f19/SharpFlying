using Newtonsoft.Json;

namespace UltraSonicLib
{
    public class UltrasonicSensor
    {
        [JsonProperty("distance")]
        public float Distance { get; protected set; }

        [JsonProperty("value")]
        public float Value { get; protected set; }
    }
}