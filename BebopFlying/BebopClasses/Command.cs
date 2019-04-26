using System;
using System.Collections.Generic;
using System.Linq;

namespace BebopFlying.BebopClasses
{
    public class Command
    {
        private static readonly int[] _seq = Enumerable.Repeat(-1, 256).ToArray();

        public Command()
        {
        }

        public List<object> Cmd { get; protected set; } = new List<object>();

        protected int CurIndex, SeqId = -1;

        public void SetData(int i, int data)
        {
            Cmd[i] = data;
            CurIndex = i + 1;
        }

        public void InsertData(int data)
        {
            Cmd.Insert(CurIndex++, data);
        }

        public void InsertData(uint data)
        {
            Cmd.Insert(CurIndex++, data);
        }

        public void InsertData(ushort data)
        {
            Cmd.Insert(CurIndex++, data);
        }

        public void InsertData(byte data)
        {
            Cmd.Insert(CurIndex++, data);
        }

        public void CopyData(byte[] data, int limit = -1)
        {
            InsertArray(limit == -1 ? data : data.Take(limit).ToArray());
        }

        protected void InsertArray(byte[] data)
        {
            foreach (byte obj in data)
                InsertData(obj);
        }

        public void InsertTuple(CommandTuple cmdTuple)
        {
            InsertData((Byte) cmdTuple.ProjectId);
            InsertData((Byte) cmdTuple.ClassId);
            InsertData((ushort) cmdTuple.CmdId);
        }

        public byte[] Export(string fmt)
        {
            var bytes = StructConverter.Pack(Cmd.Cast<object>().ToArray(), true, out string internalFmt);

            if (internalFmt != fmt)
                throw new Exception("FK");
            return bytes;
        }

        public int SequenceId
        {
            set
            {
                SeqId = value;
                if (_seq[value] == -1)
                    _seq[value] = 0;
                else
                    _seq[value] = (_seq[value] + 1) % 256;
            }
            get
            {
                if (SeqId == -1)
                    return -1;
                return _seq[SeqId];
            }
        }
    }
}