﻿using System;
using System.Collections;
using System.Collections.Generic;
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
        public static Thread AbortThread;
        public static bool Fly = true;

        private static void Main(string[] args)
        {
            const int width = 640, height = 360;

            Bebop bebop = new Bebop();
            if (bebop.Connect() == ConnectionStatus.Success)
            {
                bebop.FlatTrim(2000);
                AbortThread = new Thread(Run);
                AbortThread.Start();

                bebop.StartVideo();

                VideoCapture capture = new VideoCapture(@"./bebop.sdp");
                StreamBuffer buffer = new StreamBuffer(capture, width, height);

                // buffer.AddService(new Canny(width, height));
                buffer.AddService(new UltraSonicService());

                buffer.Start();

                bebop.TakeOff();

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

                        if (Fly)
                            bebop.Move(v);
                        //Console.WriteLine(v.ToString());
                    }
                }

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
                if (test.KeyChar == 'E' || test.KeyChar == 'e')
                {
                    Console.WriteLine("WE FLY BOIS");
                    // Fly = !Fly;
                }
                else if (test.KeyChar == 'Q' || test.KeyChar == 'q')
                {
                    return;
                }

                Thread.Sleep(150);
            }
        }
    }
}