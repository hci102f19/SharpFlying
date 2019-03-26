using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Mime;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using EdgyLib.Exceptions;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Geometry.Dampening;
using ServiceLib;
using Geometry.Base;
using Geometry.Exceptions;
using Point = Geometry.Base.Point;

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

        public void ProcessFrame(Image<Bgr, Byte> frame)
        {
            // Clear lines
            using (Mat edges = new Mat())
            {
                //TODO: Taf plz fix mem-leak :i

                CvInvoke.Canny(frame, edges, CannyThreshold, CannyThreshold * CannyThresholdModifier, 3);


                VectorOfPointF vector = new VectorOfPointF();
                CvInvoke.HoughLines(edges, vector, 2, Math.PI / 180, HoughLinesTheta);

                if (vector.Size == 0)
                    return;

                try
                {
                    CalculateTheta(vector.Size);
                }
                catch (TooManyLinesException e) // Generic Exception TooManyException
                {
                    return;
                }

                List<Line> lines = GetLines(vector);


                //Calculate Cluster
                Clustering(lines, frame);
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

        protected List<Line> GetLines(VectorOfPointF vector)
        {
            List<Line> lines = new List<Line>();

            for (int i = 0; i < vector.Size; i++)
            {
                try
                {
                    lines.Add(new Line(vector[i]));
                }
                catch (InvalidLineException e)
                {
                }
            }

            return lines;
        }

        protected void Clustering(List<Line> lines, Image<Bgr, Byte> frame)
        {
            List<Point> intersections = new List<Point>();

            for (int i = lines.Count - 1; i >= 0; i--)
            {
                Line inLine = lines[i];
                for (int j = lines.Count - 1; j >= 0; j--)
                {
                    Line cmpLine = lines[j];
                    if (inLine == cmpLine)
                        continue;

                    Point intersection = inLine.Intersect(cmpLine);

                    if (intersection != null && !intersections.Contains(intersection))
                        intersections.Add(intersection);

                }
            }

            //            foreach (Line inLine in lines)
            //            {
            //                foreach (Line cmpLine in lines)
            //                {
            //                    if (inLine == cmpLine)
            //                        continue;
            //
            //                    Point intersection = inLine.Intersect(cmpLine);
            //
            //                    if (intersection != null && !intersections.Contains(intersection))
            //                        intersections.Add(intersection);
            //                }
            //            }

            foreach (Point point in intersections)
            {
                CvInvoke.Circle(frame, point.AsPoint(), 2, new MCvScalar(0, 0, 255), -1);
            }
        }
    }
}