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
        protected int Blur = 3;

        protected Image<Bgr, byte> LastFrame = null;

        protected Size Size;

        protected Stack<Image<Bgr, byte>> Stack = new Stack<Image<Bgr, byte>>(1);
        protected VideoCapture Stream;

        protected Buffer(VideoCapture stream, int width, int height)
        {
            Stream = stream;
            Size = new Size(width, height);
        }

        public bool IsRunning { get; protected set; } = true;


        public Image<Bgr, byte> GetLastFrame()
        {
            if (!IsRunning) return null;
            Retry:
            while (Stack.Count == 0 && IsRunning) Thread.Yield();

            if (Stack.Count > 0 && Stack.Peek() == null && IsRunning)
            {
                Stack.Pop();
                goto Retry;
            }

            if (IsRunning)
                return Stack.Peek();
            return null;
        }

        public Image<Bgr, byte> PopLastFrame()
        {
            if (!IsRunning) return null;
            Retry:
            while (Stack.Count == 0 && IsRunning) Thread.Yield();

            if (Stack.Count > 0 && Stack.Peek() == null && IsRunning)
            {
                Stack.Pop();
                goto Retry;
            }

            if (IsRunning)
                return Stack.Pop();
            return null;
        }

        protected Mat ProcessFrame(Mat mat)
        {
            CvInvoke.Resize(mat, mat, Size);
            CvInvoke.GaussianBlur(mat, mat, new Size(Blur, Blur), 0);

            return mat;
        }

        public void Kill()
        {
            IsRunning = false;
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