using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;

namespace VidBuffLib
{
    class StreamBuffer : Buffer
    {
        public StreamBuffer(VideoCapture stream, int width = 640, int height = 360) : base(stream, width, height)
        {
        }
    }
}
