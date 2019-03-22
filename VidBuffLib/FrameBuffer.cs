using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV.Structure;

namespace VidBuffLib
{
    public class FrameBuffer : Buffer
    {
        protected double fps, sleepTimer = 0;

        public FrameBuffer(VideoCapture stream, int width = 640, int height = 360) : base(stream, width, height)
        {
            fps = stream.GetCaptureProperty(CapProp.Fps);
            
            sleepTimer = 1 / fps;
        }

        protected void Sleep(TimeSpan executionTime)
        {
            int sleep = (int)(sleepTimer * 1000) - executionTime.Milliseconds;
            if (sleep > 0)
                Thread.Sleep(sleep);
        }

        protected override void Run()
        {
            Mat frame = stream.QueryFrame();

            while (frame != null && isRunning)
            {
                DateTime startTime = DateTime.Now;
                using (frame = ProcessFrame(frame))
                {
                    lastFrame = frame.Clone().ToImage<Bgr, byte>();
                }
                frame = stream.QueryFrame();
                GC.Collect(1);
                Sleep(DateTime.Now - startTime);
            }

            isRunning = false;
            lastFrame = null;
        }
    }
}