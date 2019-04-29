using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BebopFlying.Sensors
{
    public class FlatTrimChanged : Sensor
    {
        public bool Updated { get; protected set; } = false;

        public FlatTrimChanged(int projectId, int classId, int cmdId) : base(projectId, classId, cmdId)
        {
        }

        public override void Parse(byte[] sensorData)
        {
            Updated = true;
        }

        public void Reset()
        {
            Updated = false;
        }
    }
}