using System;
using Aardvark.Base;
using SDPoint = System.Drawing.Point;

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

        public SDPoint AsPoint()
        {
            return new SDPoint((int)X, (int)Y);
        }

        public override string ToString()
        {
            return "X: " + X + ", Y: " + Y;
        }
    }
}