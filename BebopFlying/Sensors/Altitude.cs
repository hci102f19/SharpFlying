using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BebopFlying.Sensors
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