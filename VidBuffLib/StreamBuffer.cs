using System;
using System.IO;
using BebopFlying;
using Emgu.CV;
using Emgu.CV.Structure;
using ZedGraph;

namespace VidBuffLib
{
    public class StreamBuffer : Buffer
    {
        private int _width, _height;
        public StreamBuffer(Bebop stream, int width, int height) : base(stream, width, height)
        {
            BebopStream = stream;
            this._width = width;
            this._height = height;
        }

        protected override void Run()
        {
            Image<Bgr, byte> frame = new Image<Bgr, Byte>(_width, _height);
            frame.Bytes = BebopStream.GetImageData();

            while (frame != null && IsRunning)
            {
                using (frame = ProcessFrame(frame.Mat).ToImage<Bgr,byte>())
                {
                    if (Stack.Count > 0)
                    {
                        Stack.Pop();
                        Stack.Push(frame);
                    }
                    else
                    {
                        Stack.Push(frame);
                    }
                }
                frame.Bytes = BebopStream.GetImageData();
            }

            IsRunning = false;
        }
    }
}