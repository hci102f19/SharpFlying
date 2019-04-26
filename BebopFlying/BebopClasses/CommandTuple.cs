using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BebopFlying.BebopClasses
{
    public class CommandTuple
    {
        public int ProjectId { get; protected set; }
        public int ClassId { get; protected set; }
        public int CmdId { get; protected set; }

        public CommandTuple(int projectId, int classId, int cmdId)
        {
            ProjectId = projectId;
            ClassId = classId;
            CmdId = cmdId;
        }

        public List<int> GetTuple()
        {
            return new List<int>()
            {
                ProjectId,
                ClassId,
                CmdId
            };
        }
    }
}