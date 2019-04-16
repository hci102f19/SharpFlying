using System;
using System.Collections.Generic;
using FlightLib;
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

        public List<Tuple<UltrasonicSensor, Vector>> GetSensors => new List<Tuple<UltrasonicSensor, Vector>>
        {
            new Tuple<UltrasonicSensor, Vector>(Front, new Vector(pitch: 50)),
            new Tuple<UltrasonicSensor, Vector>(Right, new Vector()),
            new Tuple<UltrasonicSensor, Vector>(Back, new Vector(pitch: -50)),
            new Tuple<UltrasonicSensor, Vector>(Left, new Vector())
        };
    }
}