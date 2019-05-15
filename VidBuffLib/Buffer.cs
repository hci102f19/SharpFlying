using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using ServiceLib;

namespace VidBuffLib
{
    public abstract class Buffer
    {
        protected int Blur = 3;

        public Thread BufferThread;

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

        public List<Service> Services { get; protected set; } = new List<Service>();

        public void AddService(Service service)
        {
            Services.Add(service);
        }

        public Image<Bgr, byte> GetLastFrame()
        {
            if (!IsRunning)
            {
                return null;
            }

            Retry:
            while (Stack.Count == 0 && IsRunning)
            {
                Thread.Yield();
            }

            if (Stack.Count > 0 && Stack.Peek() == null && IsRunning)
            {
                Stack.Pop();
                goto Retry;
            }

            return IsRunning ? Stack.Peek() : null;
        }

        public Image<Bgr, byte> PopLastFrame()
        {
            if (!IsRunning)
            {
                return null;
            }

            Retry:
            while (Stack.Count == 0 && IsRunning)
            {
                Thread.Yield();
            }

            if (Stack.Count > 0 && Stack.Peek() == null && IsRunning)
            {
                Stack.Pop();
                goto Retry;
            }

            return IsRunning ? Stack.Pop() : null;
        }

        protected Mat ProcessFrame(Mat mat)
        {
            CvInvoke.Resize(mat, mat, Size);
            CvInvoke.GaussianBlur(mat, mat, new Size(Blur, Blur), 0);

            return mat;
        }

        public void Stop()
        {
            foreach (Service s in Services)
            {
                s.Stop();
            }

            IsRunning = false;
        }

        public void Start()
        {
            //Start all connected services
            foreach (Service service in Services)
            {
                service.Start();
            }

            BufferThread = new Thread(Run);
            BufferThread.Start();
            //Task.Factory.StartNew(Run);
        }

        protected virtual void Run()
        {
            throw new NotImplementedException();
        }

        public void TransmitFrame(Image<Bgr, byte> frame)
        {
            foreach (Service service in Services)
            {
                service.Input(frame);
            }
        }
    }
}