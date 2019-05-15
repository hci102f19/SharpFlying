using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Threading;
using BebopFlying;
using EdgyLib;
using Emgu.CV;
using Emgu.CV.CvEnum;
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
        private static void Main(string[] args)
        {
            const int width = 640, height = 360;

/*            var us = new UltraSonicService();
            us.Start();
            while (true)
            {
                if (us.GetLatestResult() != null)
                {

                    //Console.WriteLine(us.GetLatestResult().Vector.ToString());
                }
            }*/
            Bebop bebop = new Bebop();
            Stopwatch sw = new Stopwatch();
            if (bebop.Connect() == ConnectionStatus.Success)
            {
                bebop.FlatTrim(2000);
                Thread abortThread = new Thread(Run);
                abortThread.Start();

                bebop.StartVideo();

                VideoCapture capture = new VideoCapture(@"./bebop.sdp");
                StreamBuffer buffer = new StreamBuffer(capture, width, height);

                buffer.AddService(new Canny(width, height));
                buffer.AddService(new UltraSonicService());

                buffer.Start();

                bebop.TakeOff();
                sw.Start();
                while (abortThread.IsAlive)
                {
                    Image<Bgr, byte> frame = buffer.PopLastFrame();
                    if (frame != null)
                    {
                        buffer.TransmitFrame(frame);
                        Vector v = new Vector {Pitch = 1};
                        foreach (Service service in buffer.Services)
                        {
                            Response r = service.GetLatestResult();
                            if (r != null && r.IsValid)
                            {
                                Vector vec = r.Vector.Copy();
                                vec.TimesConstant(r.Confidence / 100);
                                
                                v.Add(vec);
                            }
                        }

                        bebop.Move(v);
                    }
                }
                sw.Stop();
                Console.WriteLine("\n" + sw.ElapsedMilliseconds + "ms");
                buffer.Stop();
                bebop.Land();
                bebop.StopVideo();
                bebop.Disconnect();
            }


            Console.ReadLine();
        }

        private static void Run()
        {
            while (true)
            {
                ConsoleKeyInfo test = Console.ReadKey();
                if (test.KeyChar == 'Q' || test.KeyChar == 'q')
                {
                    return;
                }

                Thread.Sleep(150);
            }
        }
    }
}