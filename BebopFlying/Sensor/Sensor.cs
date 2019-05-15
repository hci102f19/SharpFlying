using System;

namespace BebopFlying.Sensor
{
    public abstract class Sensor
    {
        protected Sensor(int projectId, int classId, int cmdId)
        {
            ProjectId = projectId;
            ClassId = classId;
            CmdId = cmdId;
        }

        public int ProjectId { get; }
        public int ClassId { get; }
        public int CmdId { get; }

        public bool Apply(int projectId, int classId, int cmdId)
        {
            return projectId == ProjectId && classId == ClassId && cmdId == CmdId;
        }

        public virtual void Parse(byte[] sensorData)
        {
            throw new NotImplementedException();
        }
    }
}