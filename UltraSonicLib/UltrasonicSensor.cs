using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UltraSonicLib
{
    public class UltrasonicSensor
    {
        [JsonProperty("distance")]
        public float Distance { get; protected set; } = 0;

        [JsonProperty("value")]
        public float Value { get; protected set; } = 0;
    }
}