using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using BebopFlying;
using EdgyLib;
using Emgu.CV;
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
            render("./Flight 1 edited.mp4", "Flight 1 post-algo.mp4");
            render("./Flight 2 edited.mp4", "Flight 2 post-algo.mp4");
            render("./Flight 3 edited.mp4", "Flight 3 post-algo.mp4");
            render("./Flight 4 edited.mp4", "Flight 4 post-algo.mp4");
        }

        private static void render(string infile, string outfile)
        {
            Console.WriteLine($"Starting {outfile}");
            //const int width = 960, height = 540;
            const int width = 640, height = 360;

            VideoCapture capture = new VideoCapture(infile);
            FrameBuffer buffer = new FrameBuffer(capture, width, height);
            Image<Bgr, byte> frame = buffer.GetNextFrame();

            VideoWriter VideoWriter = new VideoWriter(outfile, -1, 24, new Size(width * 2, height * 2), true);

            Canny c = new Canny(width, height);


            while (frame != null)
            {
                Bitmap bitmap = new Bitmap(width * 2, height * 2);

                Image<Bgr, byte> canny = c.ProcessFrame(frame);
                Image<Bgr, byte> clustering = c.HoughFrame(canny, frame);


                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(frame.Bitmap, 0, 0);
                    g.DrawImage(canny.Bitmap, width, 0);

                    g.DrawImage(clustering.Bitmap, width, height);
                }

                Image<Bgr, byte> imageResult = new Image<Bgr, byte>(bitmap);

                VideoWriter.Write(imageResult.Mat);

                frame = buffer.GetNextFrame();
            }

            Console.WriteLine("DONE!");
        }
    }
}