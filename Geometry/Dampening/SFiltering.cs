using System;
using System.Collections.Generic;
using System.Linq;
using Aardvark.Base;
using Geometry.Base;

namespace Geometry.Dampening
{
    /// <summary>
    ///     Small Filtering
    ///     An Naïve approach to GPS smoothing
    /// </summary>
    public class SFiltering
    {
        protected List<Point> CurrentPoints = new List<Point>();
        protected double DeviationMax = 0.1;

        protected int HistorySize = 6;
        protected List<Point> RejectedPoints = new List<Point>();
        protected double XMax;
        protected double YMax;


        public SFiltering(double xMax, double yMax)
        {
            XMax = xMax;
            YMax = yMax;
        }

        protected Point PointToPercent(Point point)
        {
            return new Point(point.X / XMax, point.Y / YMax);
        }

        protected bool Deviate(List<Point> points, Point point)
        {
            Point percentPoint = PointToPercent(point);

            foreach (Point listPoint in points)
            {
                Point listPercentagePoint = PointToPercent(listPoint);
                if (Math.Abs(listPercentagePoint.X - percentPoint.X) >= DeviationMax ||
                    Math.Abs(listPercentagePoint.Y - percentPoint.Y) >= DeviationMax)
                    return true;
            }

            return false;
        }

        public Point GetMean()
        {
            if (CurrentPoints.Count > 0)
                return new Point(
                    CurrentPoints.Select(point => point.X).Average(),
                    CurrentPoints.Select(point => point.Y).Average()
                );
            return new Point(0, 0);
        }

        protected List<Point> GetLastHistory(List<Point> points)
        {
            points.Reverse();
            points = points.Take(HistorySize).ToList();
            points.Reverse();

            return points;
        }

        public bool Add(Point point)
        {
            if (point.X < 0 || point.X > XMax || point.Y < 0 || point.Y > YMax)
            {
            }
            else if (!CurrentPoints.Any())
            {
                CurrentPoints.Add(point);
            }
            else if (!Deviate(CurrentPoints, point))
            {
                RejectedPoints.Clear();
                CurrentPoints = GetLastHistory(CurrentPoints);
                CurrentPoints.Add(point);
            }
            else
            {
                RejectedPoints = GetLastHistory(RejectedPoints);
                RejectedPoints.Add(point);

                if (RejectedPoints.Count >= HistorySize && !Deviate(RejectedPoints, point))
                {
                    Console.WriteLine("SETTING NEW POINTS LIST!");
                    CurrentPoints = RejectedPoints.CopyToList();
                    RejectedPoints.Clear();
                    return false;
                }
            }

            return true;
        }
    }
}