using System;
using DBSCAN;
using System.Collections.Generic;
using EdgyLib.Exceptions;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Geometry.Dampening;
using ServiceLib;
using Geometry.Base;
using Geometry.Exceptions;
using Geometry.Extended;

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
            float modifier = 1;
            if (LastFrameCount != null)
            {
                modifier = ((float)LastFrameCount / lines) * 2;

                if (modifier <= 1)
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
                    Line line = new Line(vector[i]);

                    if (line.IsValid())
                        lines.Add(line);
                }
                catch (InvalidLineException e)
                {
                }
            }

            return lines;
        }

        protected void Clustering(List<Line> lines, Image<Bgr, Byte> frame)
        {
            List<PointContainer> intersections = new List<PointContainer>();

            foreach (Line inLine in lines)
            {
                foreach (Line cmpLine in lines)
                {
                    if (inLine == cmpLine)
                        continue;

                    Point intersection = inLine.Intersect(cmpLine);

                    //if (intersection != null && !intersections.Contains(intersection))
                    if (intersection != null)
                        intersections.Add(new PointContainer(intersection));
                }
            }


            if (intersections.Count > 0)
            {
                var clusters = DBSCAN.DBSCAN.CalculateClusters(
                    intersections,
                    epsilon: 20,
                    minimumPointsPerCluster: (int)Math.Round(0.1 * intersections.Count, 0)
                );
                Random r = new Random();

                foreach (var claster in clusters.Clusters)
                {
                    MCvScalar Color = new MCvScalar(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255));
                    foreach (var c in claster.Objects)
                    {
                        CvInvoke.Circle(frame, c.Point.AsPoint(), 2, Color, -1);
                    }
                }
            }


            //            foreach (PointContainer pointContainer in intersections)
            //            {
            //                CvInvoke.Circle(frame, pointContainer.Point.AsPoint(), 2, new MCvScalar(0, 0, 255), -1);
            //            }
        }
    }
}