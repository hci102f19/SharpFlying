using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using VidBuffLib;

namespace DroneVision
{
    class Program
    {
        static void Main(string[] args)
        {
            int width = 640, height = 480;

            VideoCapture videoCapture = new VideoCapture(@"./bebop.sdp");
            StreamBuffer streamBuffer = new StreamBuffer(videoCapture, width, height);

            streamBuffer.Start();

            while (streamBuffer.IsRunning)
            {
                var frame = streamBuffer.PopLastFrame();
                if (frame != null)
                {
                    CvInvoke.Imshow("frame", frame);
                    CvInvoke.WaitKey(1);
                }
            }

            Console.ReadLine();
        }
    }
}