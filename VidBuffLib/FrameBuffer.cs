using System;
using System.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace VidBuffLib
{
    public class FrameBuffer : Buffer
    {
        protected double Fps, SleepTimer;

        public FrameBuffer(VideoCapture stream, int width, int height) : base(stream, width, height)
        {
            Fps = stream.GetCaptureProperty(CapProp.Fps);

            SleepTimer = 1 / Fps;
        }

        protected void Sleep(TimeSpan executionTime)
        {
            var sleep = (int)(SleepTimer * 1000) - executionTime.Milliseconds;
            if (sleep > 0)
                Thread.Sleep(sleep);
        }

        protected override void Run()
        {
            var frame = Stream.QueryFrame();

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

                frame = Stream.QueryFrame();


                Sleep(DateTime.Now - startTime);
            }

            isRunning = false;
        }
    }
}