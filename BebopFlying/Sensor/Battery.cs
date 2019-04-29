using System;

namespace BebopFlying.Sensor
{
    public class Battery : Sensor
    {
        public int Percentage { get; protected set; } = 100;

        public Battery(int projectId, int classId, int cmdId) : base(projectId, classId, cmdId)
        {
        }

        public override void Parse(byte[] sensorData)
        {
            Percentage = (byte) sensorData[0];
            Console.WriteLine("Battery: {0}%", Percentage);
        }
    }
}