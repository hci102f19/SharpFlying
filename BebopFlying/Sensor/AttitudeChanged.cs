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


        protected double BytesToDegrees(byte[] bytes, int idx)
        {
            float radians = BitConverter.ToSingle(bytes, idx);

            return (radians * 180) / Math.PI;
        }

        public override void Parse(byte[] sensorData)
        {
            RollChanged = BytesToDegrees(sensorData, 0);
            PitchChanged = BytesToDegrees(sensorData, 4);
            YawChanged = BytesToDegrees(sensorData, 8);
        }
    }
}