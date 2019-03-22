using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV.Structure;

namespace VidBuffLib
{
    public abstract class Buffer
    {
        protected VideoCapture stream;

        protected Size size;

        protected int blur = 3;

        protected Image<Bgr, Byte> lastFrame = null;

        public bool isRunning { get; protected set; } = true;

        public Buffer(VideoCapture stream, int width = 640, int height = 360)
        {
            this.stream = stream;
            this.size = new Size(width, height);
        }

        public Image<Bgr, Byte> GetLastFrame()
        {
            return lastFrame;
        }

        public Image<Bgr, Byte> PopLastFrame()
        {
            Image<Bgr, Byte> tmpFrame = lastFrame;
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
