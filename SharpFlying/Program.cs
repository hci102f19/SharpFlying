﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using VidBuffLib;

namespace SharpFlying
{
    class Program
    {
        static void Main(string[] args)
        {
            VideoCapture capture = new VideoCapture(@"./1b946788f58e474e883414e7d694adc0.mp4");
            FrameBuffer frameBuffer = new FrameBuffer(capture);
            frameBuffer.Start();

            while (frameBuffer.isRunning)
            {
                Image<Bgr, Byte> frame = frameBuffer.PopLastFrame();
                if (frame != null)
                {
                    CvInvoke.Imshow("frame", frame);
                    CvInvoke.WaitKey(1);
                }
            }
        }
    }
}