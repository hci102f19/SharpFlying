using Emgu.CV;

namespace VidBuffLib
{
    internal class StreamBuffer : Buffer
    {
        public StreamBuffer(VideoCapture stream, int width, int height) : base(stream, width, height)
        {
        }
    }
}