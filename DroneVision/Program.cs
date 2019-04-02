using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;

namespace DroneVision
{
    class Program
    {
        static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("OPENCV_FFMPEG_CAPTURE_OPTIONS", "protocol_whitelist;file,rtp,udp", EnvironmentVariableTarget.User);
            VideoCapture videoCapture = new VideoCapture(@"./bebop.sdp");

            var frame = videoCapture.QueryFrame();

            while (frame != null)
            {
                using (frame)
                {
                    CvInvoke.Imshow("frame", frame);
                    CvInvoke.WaitKey(1);
                }

                frame = videoCapture.QueryFrame();
            }
        }
    }
}