namespace BebopFlying.Model
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
    }
}