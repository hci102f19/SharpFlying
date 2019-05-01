using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DBSCAN;
using EdgyLib.Container;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using FlightLib;
using Geometry.Base;
using Geometry.Dampening;
using Geometry.Extended;
using RenderGeometry.Base;
using ServiceLib;

namespace EdgyLib
{
    public class Canny : Service
    {
        protected BoxContainer BoxContainer;
        protected int CannyThreshold = 55;
        protected int CannyThresholdModifier = 3;
        protected double Confidence;

        protected Image<Bgr, byte> CurrentFrame;

        protected SFiltering Filtering;

        protected int HoughLinesTheta = 150;

        protected int? LastFrameCount;
        protected Response LatestResponse = new Response(false, null, 0);

        protected int LineMax = 100;

        protected int LowerLineThreshold = 20;
        protected int ThetaModifier = 5;
        protected int UpperLineThreshold = 75;

        public Canny(int width, int height)
        {
            BoxContainer = new BoxContainer(width, height);
            Filtering = new SFiltering(width, height);
        }

        public override void Input(Image<Bgr, byte> frame)
        {
            if (CurrentFrame == null)
            {
                CurrentFrame = frame;
            }
        }

        protected override void Run()
        {
            while (IsRunning)
            {
                if (CurrentFrame == null)
                {
                    // TODO: Might be looked into
                    Thread.Sleep(1);
                    continue;
                }

                Image<Bgr, byte> frame = CurrentFrame;
                CurrentFrame = null;

                using (Mat edges = new Mat())
                {
                    CvInvoke.Canny(frame, edges, CannyThreshold, CannyThreshold * CannyThresholdModifier, 3);


                    VectorOfPointF vector = new VectorOfPointF();
                    CvInvoke.HoughLines(edges, vector, 2, Math.PI / 180, HoughLinesTheta);

                    int lines = vector.Size;

                    if (lines == 0)
                    {
                        continue;
                    }

                    CalculateTheta(lines);

                    if (lines < LineMax)
                    {
                        Clustering(GetLines(vector), frame);
                    }
                }
            }
        }

        protected void CalculateTheta(int lines)
        {
            double modifier = 1;
            if (LastFrameCount != null)
            {
                modifier = (double) LastFrameCount / lines * 2;

                if (modifier <= 1)
                {
                    modifier = 1;
                }
            }

            if (lines < LowerLineThreshold && HoughLinesTheta > ThetaModifier * modifier)
            {
                HoughLinesTheta -= (int) Math.Round(ThetaModifier * modifier, 0);
                Console.WriteLine("Not enough data, decreasing l_theta to " + HoughLinesTheta);
            }
            else if (lines > UpperLineThreshold)
            {
                HoughLinesTheta += (int) Math.Round(ThetaModifier * modifier, 0);
                Console.WriteLine("Too much data, increasing l_theta to " + HoughLinesTheta);
            }

            LastFrameCount = lines;
        }

        protected List<Line> GetLines(VectorOfPointF vector)
        {
            List<Line> lines = new List<Line>();

            for (int i = 0; i < vector.Size; i++)
            {
                Line line = new Line(vector[i]);

                if (line.IsValid())
                {
                    lines.Add(line);
                }
            }

            return lines;
        }

        protected void Clustering(List<Line> lines, Image<Bgr, byte> frame)
        {
            List<Point> intersections = new List<Point>();

            foreach (Line inLine in lines)
            foreach (Line cmpLine in lines)
            {
                if (inLine == cmpLine)
                {
                    continue;
                }

                Point intersection = inLine.Intersect(cmpLine);

                if (intersection != null && !intersections.Contains(intersection))
                {
                    intersections.Add(intersection);
                }
            }


            if (intersections.Count > 0)
            {
                ClusterSet clusters = DBSCAN.DBSCAN.CalculateClusters(
                    intersections.Select(p => new PointContainer(p)).ToList(),
                    20,
                    (int) Math.Round(0.1 * intersections.Count, 0)
                );

                if (clusters.IsValid())
                {
                    if (Filtering.Add(clusters.GetBestCluster().GetMean()))
                    {
                        if (Confidence < 1)
                        {
                            Confidence = Confidence >= 100 ? 100 : Confidence + 0.25f;
                        }
                    }
                    else
                    {
                        Confidence = 1;
                    }
                }

                Vector vector = BoxContainer.Hit(Filtering.GetMean());

                LatestResponse = !vector.IsNull()
                    ? new Response(true, BoxContainer.Hit(Filtering.GetMean()), Confidence)
                    : new Response(false, null);
            }
        }

        public override Response GetLatestResult()
        {
            return LatestResponse;
        }
    }
}