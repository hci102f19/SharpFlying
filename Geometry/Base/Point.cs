using System;
using Aardvark.Base;

namespace Geometry.Base
{
    public class Point
    {
        internal V2d InternalPoint;

        public Point(V2d point) : this(point.X, point.Y)
        {
        }

        public Point(double x, double y)
        {
            InternalPoint = new V2d(x, y);
        }

        public double X => InternalPoint.X;
        public double Y => InternalPoint.Y;

        public double Distance(Point point)
        {
            var xDist = point.X - X;
            var yDist = point.Y - Y;

            return Math.Sqrt(xDist * xDist + yDist * yDist);
        }

        public override string ToString()
        {
            return "X: " + X + ", Y: " + Y;
        }
    }
}