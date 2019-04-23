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
            Bebop bebop = new Bebop(30);

            if (bebop.Connect() == ConnectionStatus.Success)
            {
                Console.WriteLine("CONNECTED!");
                bebop.TakeOff();

                Console.WriteLine("STARTING SLEEP");

                bebop.SmartSleep(2500);

                Console.WriteLine("ENDING SLEEP");

                bebop.Land();

                Console.WriteLine("STARTING SLEEP");
                bebop.SmartSleep(1500);
                Console.WriteLine("ENDING SLEEP");

                bebop.Disconnect();
            }
        }

        //        private static void Main(string[] args)
        //        {
        //            const int width = 640, height = 480;
        //            int FrameCount = 0;
        //
        //            VideoCapture video = new VideoCapture(@"./2e792cbf847942eeb8147a5e52e0fef2.mp4");
        //            FrameBuffer buffer = new FrameBuffer(video, width, height);
        //
        //            buffer.AddService(new Canny(width, height, true));
        //            buffer.Start();
        //
        //            while (buffer.IsRunning)
        //            {
        //                Image<Bgr, byte> frame = buffer.PopLastFrame();
        //                if (frame != null)
        //                {
        //                    buffer.TransmitFrame(frame);
        //
        //                    CvInvoke.Imshow("frame", frame);
        //                    CvInvoke.WaitKey(1);
        //                    
        //                    //frame.ToBitmap().Save("./output/clean/clean_" + FrameCount + ".png");
        //                    FrameCount++;
        //                }
        //            }
        //        }

        //        private static void Main(string[] args)
        //        {
        //            const int width = 640, height = 480;
        //
        //            VideoCapture capture = new VideoCapture(@"./bebop.sdp");
        //
        //            Bebop bebop = new Bebop(30);
        //
        //            if (bebop.Connect() == ConnectionStatus.Success)
        //            {
        //                StreamBuffer buffer = new StreamBuffer(capture, width, height);
        //
        //                buffer.AddService(new Canny(width, height, true));
        //                buffer.AddService(new UltraSonicService());
        //                // buffer.AddService(new WiFiService());
        //
        //                buffer.Start();
        //
        //                while (buffer.IsRunning)
        //                {
        //                    Image<Bgr, byte> frame = buffer.PopLastFrame();
        //                    if (frame != null)
        //                    {
        //                        buffer.TransmitFrame(frame);
        //
        //                        Console.WriteLine(buffer.CalculateMovement());
        //
        //                        foreach (Service service in buffer.Services)
        //                        {
        //                            Response r = service.GetLatestResult();
        //                            if (r != null && r.IsValid)
        //                            {
        //                                Console.WriteLine(r.Vector);
        //                            }
        //                        }
        //
        //                        //CvInvoke.Imshow("frame", frame);
        //                        //CvInvoke.WaitKey(1);
        //                    }
        //                }
        //            }
        //        }
    }
}