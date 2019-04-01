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

            frameBuffer.AddService(new Canny(width, height, true));

            frameBuffer.Start();

            while (frameBuffer.IsRunning)
            {
                var frame = frameBuffer.PopLastFrame();
                if (frame != null)
                {
                    frameBuffer.TransmitFrame(frame);

                    foreach (var service in frameBuffer.Services)
                    {
                        var r = service.GetLatestResult();
                        if (r != null && r.IsValid)
                            Console.WriteLine(r.Vector);
                    }

                    // CvInvoke.Imshow("frame", frame);
                    // CvInvoke.WaitKey(1);
                }
            }
        }
    }
}