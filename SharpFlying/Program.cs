using System;
using BebopFlying;
using EdgyLib;
using Emgu.CV;
using Emgu.CV.Structure;
using Flight.Enums;
using ServiceLib;
using UltraSonicLib;
using VidBuffLib;

namespace SharpFlying
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const int width = 640, height = 480;

            VideoCapture capture = new VideoCapture(@"./bebop.sdp");

            Bebop bebop = new Bebop(30);

            if (bebop.Connect() == ConnectionStatus.Success)
            {
                StreamBuffer buffer = new StreamBuffer(capture, width, height);

                buffer.AddService(new Canny(width, height, true));
                buffer.AddService(new UltraSonicService());
                // buffer.AddService(new WiFiService());

                buffer.Start();

                while (buffer.IsRunning)
                {
                    Image<Bgr, byte> frame = buffer.PopLastFrame();
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
}