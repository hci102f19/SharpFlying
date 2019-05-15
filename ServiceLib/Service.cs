using System;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;

namespace ServiceLib
{
    public abstract class Service
    {
        protected bool IgnoreInput = false;
        public Thread BackgroundThread { get; protected set; }

        public bool IsRunning { get; protected set; } = true;

        public void Start()
        {
            BackgroundThread = new Thread(Run);
            BackgroundThread.Start();
        }

        protected virtual void Run()
        {
            throw new NotImplementedException();
        }

        public virtual void Input(Image<Bgr, byte> frame)
        {
            if (!IgnoreInput)
            {
                throw new NotImplementedException();
            }
        }

        public virtual Response GetLatestResult()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            IsRunning = false;
        }
    }
}