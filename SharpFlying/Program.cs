using System;
using BebopFlying;
using EdgyLib;
using Emgu.CV;
using ServiceLib;
using UltraSonicLib;
using VidBuffLib;
using WiFiLib;

namespace SharpFlying
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            int width = 640, height = 480;

            var capture = new VideoCapture(@"./bebop.sdp");

            var bebop = new Bebop(30);
            bebop.Connect();
            var buffer = new StreamBuffer(capture, width, height);

            buffer.AddService(new Canny(width, height, true));
            buffer.AddService(new UltraSonicService());
            // buffer.AddService(new WiFiService());

            buffer.Start();

            while (buffer.IsRunning)
            {
                var frame = buffer.PopLastFrame();
                if (frame != null)
                {
                    buffer.TransmitFrame(frame);

                    foreach (Service service in buffer.Services)
                    {
                        Response r = service.GetLatestResult();
                        if (r != null && r.IsValid)
                            Console.WriteLine(r.Vector);
                    }

                    //CvInvoke.Imshow("frame", frame);
                    //CvInvoke.WaitKey(1);
                }
            }
        }
    }
}