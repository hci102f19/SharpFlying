using System;
using BebopFlying;
using EdgyLib;
using Emgu.CV;
using Flight.Enums;
using ServiceLib;
using VidBuffLib;
using WiFiLib;

namespace SharpFlying
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            WiFiService myWiFiService = new WiFiService();
            myWiFiService.Start();

            Console.ReadLine();
            myWiFiService.Stop();
        }

        //        private static Bebop bebop;
        //        private static StreamBuffer buffer;
        //        private static void Main(string[] args)
        //        {
        //            int width = 640, height = 480;
        //
        //            var capture = new VideoCapture(@"./bebop.sdp");
        //
        //            Bebop bebop = new Bebop(30);
        //            bebop.Connect();
        //            var buffer = new StreamBuffer(capture, width, height);
        //            buffer.AddService(new Canny(width, height, true));
        //            buffer.Start();
        //
        //            while (buffer.IsRunning)
        //            {
        //                var frame = buffer.PopLastFrame();
        //                if (frame != null)
        //                {
        //                    buffer.TransmitFrame(frame);
        //
        //                    foreach (var service in buffer.Services)
        //                    {
        //                        var r = service.GetLatestResult();
        //                        //if (r != null && r.IsValid)
        //                            //Console.WriteLine(r.Vector);
        //                    }
        //
        //                    //CvInvoke.Imshow("frame", frame);
        //                    //CvInvoke.WaitKey(1);
        //                }
        //            }
        //        }
    }
}