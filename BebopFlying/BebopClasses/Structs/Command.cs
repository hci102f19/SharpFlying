namespace BebopFlying.BebopClasses.Structs
{
    public struct Command
    {
        public byte[] cmd;
        public int size;
    }

    public struct BebopData
    {
        public byte DataType;
        public byte BufferID;
        public byte PacketSequenceID;
        public int PacketSize;
        public byte[] data;
    }
}