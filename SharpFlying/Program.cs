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
            int width = 640, height = 480;

            var capture = new VideoCapture(@"./video.v2.mp4");
            var frameBuffer = new FrameBuffer(capture, width, height);
            frameBuffer.Start();

            var canny = (Service)new Canny(width, height);
            canny.Start();

            while (frameBuffer.isRunning)
                using (var frame = frameBuffer.PopLastFrame())
                {
                    if (frame != null)
                    {
                        canny.Input(frame);
                        Response r = canny.GetLatestResult();

                        if (r != null && r.IsValid)
                            Console.WriteLine(r.Vector);

                        CvInvoke.Imshow("frame", frame);
                        CvInvoke.WaitKey(1);
                    }
                }
        }
    }
}