using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VidBuffLib
{
    public abstract class Buffer
    {
        protected VideoCapture stream;

        protected Size size;

        protected int blur = 3;

        protected Mat lastFrame = null;

        public bool isRunning { get; protected set; } = true;

        public Buffer(VideoCapture stream, int width = 640, int height = 360)
        {
            this.stream = stream;
            this.size = new Size(width, height);
        }

        public Mat GetLastFrame()
        {
            return lastFrame;
        }

        public Mat PopLastFrame()
        {
            Mat tmpFrame = lastFrame;
            lastFrame = null;

            return tmpFrame;
        }

        protected Mat ProcessFrame(Mat mat)
        {
            CvInvoke.Resize(mat, mat, size);
            CvInvoke.GaussianBlur(mat, mat, new Size(blur, blur), 0);

            return mat;
        }

        public void Kill()
        {
            isRunning = false;
        }

        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                Run();
            });
        }

        protected virtual void Run()
        {
            throw new NotImplementedException();
        }

    }
}
