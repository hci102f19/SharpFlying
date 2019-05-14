using System;
using Newtonsoft.Json;

namespace UltraSonicLib
{
    public class UltrasonicSensor
    {
        protected static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        protected double InternalDistance = 0;

        [JsonProperty("distance")]
        //TODO: Should we do this?
        public double Distance
        {
            get => InternalDistance;
            protected set => InternalDistance = Clamp(value, 0, 200);
        }

        [JsonProperty("value")] public double Value { get; protected set; }
    }
}