using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VidBuffLib
{
    public class FrameBuffer : Buffer
    {
        protected double fps;
        protected double sleepTimer = 0;

        public FrameBuffer(VideoCapture stream, int width = 640, int height = 360) : base(stream, width, height)
        {
            fps = stream.GetCaptureProperty(CapProp.Fps);
            sleepTimer = 1 / fps;
        }

        protected void Sleep(TimeSpan executionTime)
        {
            Thread.Sleep((int)(sleepTimer * 1000) - executionTime.Milliseconds);
        }

        protected override void Run()
        {
            Mat frame = stream.QueryFrame();

            while (frame != null && isRunning)
            {
                DateTime startTime = DateTime.Now;
                frame = this.ProcessFrame(frame);

                lastFrame = frame;

                frame = stream.QueryFrame();

                Sleep(DateTime.Now - startTime);
            }

            isRunning = false;
            lastFrame = null;
        }
    }
}