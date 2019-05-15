using System;

namespace BebopFlying.Sensor
{
    public class Battery : Sensor
    {
        public Battery(int projectId, int classId, int cmdId) : base(projectId, classId, cmdId)
        {
        }

        public int Percentage { get; protected set; } = 100;

        public override void Parse(byte[] sensorData)
        {
            Percentage = sensorData[0];
            Console.WriteLine("Battery: {0}%", Percentage);
        }
    }
}