using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using EdgyLib.Exceptions;
using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Geometry.Dampening;
using ServiceLib;

namespace EdgyLib
{
    public class Canny : Service
    {
        protected int CannyThreshold = 55;
        protected int CannyThresholdModifier = 3;

        protected int HoughLinesTheta = 150;
        protected int ThetaModifier = 5;

        protected int? LastFrameCount = null;

        protected int LowerLineThreshold = 20;
        protected int UpperLineThreshold = 75;

        protected int LineMax = 100;

        protected SFiltering filtering = new SFiltering();

        public Image<Bgr, Byte> ProcessFrame(Image<Bgr, Byte> frame)
        {
            // Clear lines
            using (Mat edges = new Mat())
            {
                //TODO: Taf plz fix mem-leak :i

                CvInvoke.Canny(frame, edges, CannyThreshold, CannyThreshold * CannyThresholdModifier, 3);


                VectorOfPointF vector = new VectorOfPointF();
                CvInvoke.HoughLines(edges, vector, 2, Math.PI / 180, HoughLinesTheta);

                if (vector.Size > 0)
                {
                    try
                    {
                        CalculateTheta(vector.Size);
                    }
                    catch (TooManyLinesException e)
                    {
                        return null;
                    }
                }


                //Calculate Cluster
                return edges.ToImage<Bgr, byte>();
            }
        }

        protected void CalculateTheta(int lines)
        {
            float modifier = 1f;
            if (LastFrameCount != null)
            {
                modifier = ((float)LastFrameCount / lines) * 2;

                if (modifier <= 0)
                    modifier = 1;
            }

            if (lines < LowerLineThreshold && HoughLinesTheta > ThetaModifier * modifier)
            {
                HoughLinesTheta -= (int)Math.Round(ThetaModifier * modifier, 0);
                Console.WriteLine("Not enough data, decreasing l_theta to " + HoughLinesTheta);
            }
            else if (lines > UpperLineThreshold)
            {
                HoughLinesTheta += (int)Math.Round(ThetaModifier * modifier, 0);
                Console.WriteLine("Too much data, increasing l_theta to " + HoughLinesTheta);
            }

            LastFrameCount = lines;

            if (LineMax < lines)
                throw new TooManyLinesException();
        }
    }
}