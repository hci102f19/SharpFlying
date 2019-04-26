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

        public void InsertData(byte data)
        {
            Cmd.Insert(CurIndex++, data);
        }

        public void InsertData(short data)
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
            foreach (int cmdVal in cmdTuple.GetTuple())
                InsertData(cmdVal);
        }

        public byte[] Export(string fmt)
        {
            return fmt == null ? StructConverter.Pack(Cmd.Cast<object>().ToArray()) : StructConverter.Pack(Cmd.Cast<object>().ToArray(), true, out fmt);
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