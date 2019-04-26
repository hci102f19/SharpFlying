using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BebopFlying.Sensors
{
    public class FlyingState : Sensor
    {
        protected static List<string> States = new List<string>() {"landed", "takingoff", "hovering", "flying", "landing", "emergency", "usertakeoff", "motor_ramping", "emergency_landing", "unknown"};
        public int State { get; protected set; } = States.Count - 1;

        public FlyingState(int projectId, int classId, int cmdId) : base(projectId, classId, cmdId)
        {
        }

        public override void Parse(byte[] sensorData)
        {
            State = (byte) sensorData[0];
        }

        public override string ToString()
        {
            return States[State];
        }
    }
}