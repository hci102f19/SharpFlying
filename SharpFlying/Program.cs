using System;
using EdgyLib;
using Emgu.CV;
using ServiceLib;
using VidBuffLib;

namespace SharpFlying
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var capture = new VideoCapture(@"./video.v2.mp4");
            var frameBuffer = new FrameBuffer(capture);
            frameBuffer.Start();

            var canny = (Service)new Canny();
            canny.Start();

            while (frameBuffer.isRunning)
                using (var frame = frameBuffer.PopLastFrame())
                {
                    if (frame != null)
                    {
                        canny.Input(frame);
                        //Console.WriteLine(canny.GetLatestResult());

                        CvInvoke.Imshow("frame", frame);
                        CvInvoke.WaitKey(1);
                    }
                }
        }
    }
}