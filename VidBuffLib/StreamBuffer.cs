using Emgu.CV;

namespace VidBuffLib
{
    internal class StreamBuffer : Buffer
    {
        public StreamBuffer(VideoCapture stream, int width = 640, int height = 360) : base(stream, width, height)
        {
        }
    }
}