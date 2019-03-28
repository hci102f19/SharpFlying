using EdgyLib;
using Emgu.CV;
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

            var canny = new Canny();

            while (frameBuffer.isRunning)
                using (var frame = frameBuffer.PopLastFrame())
                {
                    if (frame != null)
                    {
                        canny.ProcessFrame(frame);
                        CvInvoke.Imshow("frame", frame);
                        CvInvoke.WaitKey(1);
                    }
                }
        }
    }
}