using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace VidBuffLib
{
    public abstract class Buffer
    {
        protected int blur = 3;

        protected Image<Bgr, byte> lastFrame = null;

        protected Size size;

        protected Stack<Image<Bgr, byte>> Stack = new Stack<Image<Bgr, byte>>(1);
        protected VideoCapture stream;

        public Buffer(VideoCapture stream, int width, int height)
        {
            this.stream = stream;
            size = new Size(width, height);
        }

        public bool isRunning { get; protected set; } = true;

        public Image<Bgr, byte> GetLastFrame()
        {
            if (!isRunning) return null;
            Retry:
            while (Stack.Count == 0 && isRunning) Thread.Yield();

            if (Stack.Count > 0 && Stack.Peek() == null && isRunning)
            {
                Stack.Pop();
                goto Retry;
            }

            if (isRunning)
                return Stack.Peek();
            return null;
        }

        public Image<Bgr, byte> PopLastFrame()
        {
            if (!isRunning) return null;
            Retry:
            while (Stack.Count == 0 && isRunning) Thread.Yield();

            if (Stack.Count > 0 && Stack.Peek() == null && isRunning)
            {
                Stack.Pop();
                goto Retry;
            }

            if (isRunning)
                return Stack.Pop();
            return null;
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
            Task.Factory.StartNew(() => { Run(); });
        }

        protected virtual void Run()
        {
            throw new NotImplementedException();
        }
    }
}