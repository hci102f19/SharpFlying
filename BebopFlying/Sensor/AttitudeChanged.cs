using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BebopFlying.Sensor
{
    public class AttitudeChanged : Sensor
    {
        public double RollChanged { get; protected set; }
        public double PitchChanged { get; protected set; }
        public double YawChanged { get; protected set; }

        public AttitudeChanged(int projectId, int classId, int cmdId) : base(projectId, classId, cmdId)
        {
        }

        public override void Parse(byte[] sensorData)
        {
            RollChanged = BitConverter.ToSingle(sensorData, 0);
            PitchChanged = BitConverter.ToSingle(sensorData, 4);
            YawChanged = BitConverter.ToSingle(sensorData, 8);
        }
    }
}