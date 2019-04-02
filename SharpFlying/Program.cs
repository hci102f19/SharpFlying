﻿using System;
using BebopFlying;
using EdgyLib;
using Emgu.CV;
using Flight.Enums;
using ServiceLib;
using VidBuffLib;

namespace SharpFlying
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            int width = 640, height = 480;

            var capture = new VideoCapture(@"C:\Users\bstaf\Documents\GitHub\SharpFlying\DroneVision\bin\Debug/bebop.sdp");
            //var frameBuffer = new FrameBuffer(capture, width, height);
            Bebop bebop = new Bebop(30);
            bebop.Connect();
            var buffer = new FrameBuffer(capture,width,height);
            buffer.AddService(new Canny(width, height, true));
            buffer.Start();
            //frameBuffer.AddService(new Canny(width, height, true));

            //frameBuffer.Start();

            while (buffer.IsRunning)
            {
                var frame = buffer.PopLastFrame();
                if (frame != null)
                {
                    buffer.TransmitFrame(frame);

                    foreach (var service in buffer.Services)
                    {
                        var r = service.GetLatestResult();
                        if (r != null && r.IsValid)
                            Console.WriteLine(r.Vector);
                    }

                    // CvInvoke.Imshow("frame", frame);
                    // CvInvoke.WaitKey(1);
                }
            }
        }
    }
}