﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EdgyLib;
using Emgu.CV;
using Emgu.CV.Structure;
using VidBuffLib;

namespace SharpFlying
{
    class Program
    {
        static void Main(string[] args)
        {
            VideoCapture capture = new VideoCapture(@"./video.v2.mp4");
            FrameBuffer frameBuffer = new FrameBuffer(capture);
            frameBuffer.Start();

            Canny canny = new Canny();

            while (frameBuffer.isRunning)
            {
                using (Image<Bgr, Byte> frame = frameBuffer.PopLastFrame())
                {
                    if (frame.Data != null)
                    {
                        canny.ProcessFrame(frame);
                        CvInvoke.Imshow("frame", frame);
                        CvInvoke.WaitKey(1);
                    }
                }
            }
        }
    }
}