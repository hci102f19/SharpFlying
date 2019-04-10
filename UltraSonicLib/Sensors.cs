using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UltraSonicLib
{
    public class Sensors
    {
        [JsonProperty("Front")]
        public UltrasonicSensor Front { get; protected set; }

        [JsonProperty("Right")]
        public UltrasonicSensor Right { get; protected set; }

        [JsonProperty("Back")]
        public UltrasonicSensor Back { get; protected set; }

        [JsonProperty("Left")]
        public UltrasonicSensor Left { get; protected set; }

    }
}
