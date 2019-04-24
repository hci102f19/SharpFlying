namespace BebopFlying.BebopClasses
{
    public class Command
    {
        private static readonly int[] _seq = new int[256];

        public Command(int size, int type = CommandSet.ARNETWORKAL_FRAME_TYPE_DATA, int id = CommandSet.BD_NET_CD_NONACK_ID)
        {
            Size = size;
            Cmd = new byte[size];

            Type = type;
            Id = id;
        }

        public byte[] Cmd { get; protected set; }
        public int Size { get; protected set; }

        protected int CurIndex = 0, Id, Type;

        public void SetData(int i, int data)
        {
            Cmd[i] = (byte) data;
            CurIndex = i + 1;
        }

        public void InsertData(int data)
        {
            Cmd[CurIndex++] = (byte) data;
        }

        public void CopyData(byte[] data, int idx)
        {
            data.CopyTo(Cmd, idx);
        }

        public int SequenceID()
        {
            return _seq[Id];
        }

        public byte[] ExportCommand()
        {
            int bufSize = Size + 7;
            byte[] buf = new byte[bufSize];

            _seq[Id] = (_seq[Id] + 1) % 256;

            buf[0] = (byte)Type;
            buf[1] = (byte)Id;
            buf[2] = (byte)SequenceID();
            buf[3] = (byte)(bufSize & 0xff);
            buf[4] = (byte)((bufSize & 0xff00) >> 8);
            buf[5] = (byte)((bufSize & 0xff0000) >> 16);
            buf[6] = (byte)((bufSize & 0xff000000) >> 24);

            Cmd.CopyTo(buf, 7);

            return buf;

        }
    }
}