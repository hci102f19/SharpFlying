using System;
using System.Collections.Generic;
using System.Linq;

namespace BebopFlying.Model
{
    public class Command
    {
        protected static readonly int[] Seq = Enumerable.Repeat(-1, 256).ToArray();
        protected int CurIndex, SeqId = -1;

        public List<object> Cmd { get; protected set; } = new List<object>();


        #region Insert data into command sequence

        public void InsertData<T>(T data)
        {
            Cmd.Insert(CurIndex++, data);
        }

        #endregion


        #region Insert structured data

        public void InsertTuple(CommandTuple cmdTuple)
        {
            InsertData((Byte) cmdTuple.ProjectId);
            InsertData((Byte) cmdTuple.ClassId);
            InsertData((ushort) cmdTuple.CmdId);
        }

        public void InsertParam(CommandParam cmdParam)
        {
            foreach (object obj in cmdParam.Parameters)
            {
                InsertData(obj);
            }
        }

        #endregion


        #region Export data

        public byte[] Export(string fmt)
        {
            byte[] bytes = StructConverter.Pack(Cmd.Cast<object>().ToArray(), true, out string internalFmt);

            if (internalFmt != fmt)
                throw new System.Exception("FK");
            return bytes;
        }

        public int SequenceId
        {
            set
            {
                SeqId = value;
                if (Seq[value] == -1)
                    Seq[value] = 0;
                else
                    Seq[value] = (Seq[value] + 1) % 256;
            }
            get
            {
                if (SeqId == -1)
                    return -1;
                return Seq[SeqId];
            }
        }

        #endregion
    }
}