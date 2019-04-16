using System;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using FlightLib;
using ServiceLib;

namespace VidBuffLib
{
    public class StreamBuffer : Buffer
    {
        public StreamBuffer(VideoCapture stream, int width, int height) : base(stream, width, height)
        {
            FixEnviromentVariable();
        }

        private void FixEnviromentVariable()
        {
            string env = Environment.GetEnvironmentVariable(
                "OPENCV_FFMPEG_CAPTURE_OPTIONS",
                EnvironmentVariableTarget.User
            );

            string envVal = "protocol_whitelist;file,rtp,udp";

            if (env == null || env != envVal)
            {
                Environment.SetEnvironmentVariable(
                    "OPENCV_FFMPEG_CAPTURE_OPTIONS",
                    envVal,
                    EnvironmentVariableTarget.User
                );


                Console.WriteLine("Setting Environment Variables.");
                Console.WriteLine("Application needs to be restarted.");
                Console.ReadLine();
                Environment.Exit(0);
            }
        }

        protected override void Run()
        {
            Mat frame = Stream.QueryFrame();

            while (!Stream.IsOpened)
            {
                Console.WriteLine("Waiting for stream to open");
                Thread.Sleep(100);
            }

            while (frame != null && IsRunning)
            {
                using (frame = ProcessFrame(frame))
                {
                    if (Stack.Count > 0)
                    {
                        Stack.Pop();
                        Stack.Push(frame.ToImage<Bgr, byte>());
                    }
                    else
                    {
                        Stack.Push(frame.ToImage<Bgr, byte>());
                    }
                }

                frame = Stream.QueryFrame();
            }

            IsRunning = false;
        }

        public Vector CalculateMovement()
        {
            Vector movement = new Vector();

            foreach (Service service in Services)
            {
                Response response = service.GetLatestResult();
                if (response != null && response.IsValid)
                {
                    Vector responseVector = response.Vector.TimesConstant(response.Confidence / 100);
                    movement.Add(responseVector);
                }
            }

            return movement;
        }
    }
}