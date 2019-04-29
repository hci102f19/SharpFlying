using System;

namespace BebopFlying.Sensor
{
    public class Altitude : Sensor
    {
        public double Value { get; protected set; } = 0;

        public Altitude(int projectId, int classId, int cmdId) : base(projectId, classId, cmdId)
        {
        }

        public override void Parse(byte[] sensorData)
        {
            Value = BitConverter.ToDouble(sensorData, 0);
        }
    }
}