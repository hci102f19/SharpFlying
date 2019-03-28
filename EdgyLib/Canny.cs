using System;
using System.Collections.Generic;
using System.Linq;
using EdgyLib.Exceptions;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Geometry.Base;
using Geometry.Dampening;
using Geometry.Exceptions;
using Geometry.Extended;
using ServiceLib;

namespace EdgyLib
{
    public class Canny : Service
    {
        protected int CannyThreshold = 55;
        protected int CannyThresholdModifier = 3;

        // TODO: Look into
        protected SFiltering filtering = new SFiltering(640, 480);

        protected int HoughLinesTheta = 150;

        protected int? LastFrameCount;

        protected int LineMax = 100;

        protected int LowerLineThreshold = 20;
        protected int ThetaModifier = 5;
        protected int UpperLineThreshold = 75;

        public void ProcessFrame(Image<Bgr, byte> frame)
        {
            // Clear lines
            using (var edges = new Mat())
            {
                CvInvoke.Canny(frame, edges, CannyThreshold, CannyThreshold * CannyThresholdModifier, 3);


                var vector = new VectorOfPointF();
                CvInvoke.HoughLines(edges, vector, 2, Math.PI / 180, HoughLinesTheta);

                if (vector.Size == 0)
                    return;

                CalculateTheta(vector.Size);

                // Check for too many lines
                if (vector.Size < LineMax)
                    Clustering(GetLines(vector), frame);
            }
        }

        protected void CalculateTheta(int lines)
        {
            float modifier = 1;
            if (LastFrameCount != null)
            {
                modifier = (float)LastFrameCount / lines * 2;

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
        }

        protected List<Line> GetLines(VectorOfPointF vector)
        {
            var lines = new List<Line>();

            for (var i = 0; i < vector.Size; i++)
            {
                var line = new Line(vector[i]);

                if (line.IsValid())
                    lines.Add(line);
            }

            return lines;
        }

        protected void Clustering(List<Line> lines, Image<Bgr, byte> frame)
        {
            var intersections = new List<Point>();

            foreach (var inLine in lines)
                foreach (var cmpLine in lines)
                {
                    if (inLine == cmpLine)
                        continue;

                    var intersection = inLine.Intersect(cmpLine);

                    if (intersection != null && !intersections.Contains(intersection))
                        intersections.Add(intersection);
                }


            if (intersections.Count > 0)
            {
                var clusters = DBSCAN.DBSCAN.CalculateClusters(
                    intersections.Select(p => new PointContainer(p)).ToList(),
                    20,
                    (int)Math.Round(0.1 * intersections.Count, 0)
                );

                if (clusters.IsValid()) filtering.Add(clusters.GetBestCluster().GetMean());

                var r = new Random();
                var Color = new MCvScalar(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255));
                CvInvoke.Circle(frame, filtering.GetMean().AsPoint(), 2, Color, -1);
            }
        }
    }
}