using System;
using DBSCAN;
using System.Collections.Generic;
using System.Linq;
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

        // TODO: Look into
        protected SFiltering filtering = new SFiltering(640, 480);

        public void ProcessFrame(Image<Bgr, Byte> frame)
        {
            // Clear lines
            using (Mat edges = new Mat())
            {
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

                Clustering(GetLines(vector), frame);
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
            List<Point> intersections = new List<Point>();

            foreach (Line inLine in lines)
            {
                foreach (Line cmpLine in lines)
                {
                    if (inLine == cmpLine)
                        continue;

                    Point intersection = inLine.Intersect(cmpLine);

                    if (intersection != null && !intersections.Contains(intersection))
                        intersections.Add(intersection);
                }
            }


            if (intersections.Count > 0)
            {
                ClusterSet clusters = DBSCAN.DBSCAN.CalculateClusters(
                    intersections.Select(p => new PointContainer(p)).ToList(),
                    epsilon: 20,
                    minimumPointsPerCluster: (int)Math.Round(0.1 * intersections.Count, 0)
                );

                if (clusters.IsValid())
                {
                    filtering.Add(clusters.GetBestCluster().GetMean());
                }

                Random r = new Random();
                MCvScalar Color = new MCvScalar(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255));
                CvInvoke.Circle(frame, filtering.GetMean().AsPoint(), 2, Color, -1);

            }
        }
    }
}