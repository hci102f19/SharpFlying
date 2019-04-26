using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BebopFlying.Sensors
{
    public abstract class Sensor
    {
        public int ProjectId { get; }
        public int ClassId { get; }
        public int CmdId { get; }

        protected Sensor(int projectId, int classId, int cmdId)
        {
            ProjectId = projectId;
            ClassId = classId;
            CmdId = cmdId;
        }

        public bool Apply(int projectId, int classId, int cmdId)
        {
            return projectId == this.ProjectId && classId == this.ClassId && cmdId == this.CmdId;
        }

        public virtual void Parse(byte[] sensorData)
        {
            throw new NotImplementedException();
        }
    }
}