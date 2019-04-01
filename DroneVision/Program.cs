using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;

namespace DroneVision
{
    class Program
    {
        static void Main(string[] args)
        {
            VideoCapture videoCapture = new VideoCapture(@"./bebop.sdp");

            var frame = videoCapture.QueryFrame();

            while (frame != null)
            {
                CvInvoke.Imshow("frame", frame);
                CvInvoke.WaitKey(1);
                frame = videoCapture.QueryFrame();
            }
        }
    }
}