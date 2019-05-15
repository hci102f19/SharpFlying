using System;

namespace BebopFlying.Sensor
{
    public class Altitude : Sensor
    {
        public Altitude(int projectId, int classId, int cmdId) : base(projectId, classId, cmdId)
        {
        }

        public double Value { get; protected set; }

        public override void Parse(byte[] sensorData)
        {
            Value = BitConverter.ToDouble(sensorData, 0);
        }
    }
}