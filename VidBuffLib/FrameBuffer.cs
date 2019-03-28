using System;
using System.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace VidBuffLib
{
    public class FrameBuffer : Buffer
    {
        protected double fps, sleepTimer;

        public FrameBuffer(VideoCapture stream, int width = 640, int height = 360) : base(stream, width, height)
        {
            fps = stream.GetCaptureProperty(CapProp.Fps);

            sleepTimer = 1 / fps;
        }

        protected void Sleep(TimeSpan executionTime)
        {
            var sleep = (int) (sleepTimer * 1000) - executionTime.Milliseconds;
            if (sleep > 0)
                Thread.Sleep(sleep);
        }

        protected override void Run()
        {
            var frame = stream.QueryFrame();

            while (frame != null && isRunning)
            {
                var startTime = DateTime.Now;
                using (frame = ProcessFrame(frame))
                {
                    if (Stack.Count > 0)
                    {
                        Stack.Pop();
                        Stack.Push(frame.ToImage<Bgr, byte>());
                    }
                    else
                    {
                        Stack.Push(frame.ToImage<Bgr, byte>());
                    }
                }

                frame = stream.QueryFrame();


                Sleep(DateTime.Now - startTime);
            }

            isRunning = false;
        }
    }
}