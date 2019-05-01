using System;
using System.Net.NetworkInformation;
using System.Threading;
using BebopFlying;
using EdgyLib;
using Emgu.CV;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using FlightLib;
using FlightLib.Enum;
using ServiceLib;
using UltraSonicLib;
using VidBuffLib;

namespace SharpFlying
{
    internal class Program
    {
        public static Thread AbortThread;

        private static void Main(string[] args)
        {
            const int width = 640, height = 360;

            Bebop bebop = new Bebop();
            if (bebop.Connect() == ConnectionStatus.Success)
            {
                AbortThread = new Thread(Run);
                AbortThread.Start();

                bebop.StartVideo();

                VideoCapture capture = new VideoCapture(@"./bebop.sdp");
                StreamBuffer buffer = new StreamBuffer(capture, width, height);

                buffer.AddService(new Canny(width, height));
                buffer.AddService(new UltraSonicService());

                buffer.Start();

                while (AbortThread.IsAlive)
                {
                    Image<Bgr, byte> frame = buffer.PopLastFrame();
                    if (frame != null)
                    {
                        buffer.TransmitFrame(frame);
                        Vector v = new Vector();

                        foreach (Service service in buffer.Services)
                        {
                            Response r = service.GetLatestResult();
                            if (r != null && r.IsValid)
                            {
                                v.Add(r.Vector);
                            }
                        }

                        Console.WriteLine(v.ToString());
                        CvInvoke.Imshow("frame", frame);
                        CvInvoke.WaitKey(1);
                    }
                }

                buffer.Stop();
                bebop.Land();
                bebop.StopVideo();
                bebop.Disconnect();
            }
        }

        private static void Run()
        {
            while (true)
            {
                Console.ReadLine();
                Console.WriteLine("ABORTING");
                return;
            }
        }
    }
}