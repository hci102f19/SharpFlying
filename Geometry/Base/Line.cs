using System;
using System.Drawing;
using Aardvark.Base;

namespace Geometry.Base
{
    public class Line
    {
        protected int AngleThreshold = 20;
        public V2d EndPoint;
        protected Line2d InternalLine;
        public V2d StartPoint;

        public Line(PointF point) : this(point.X, point.Y)
        {
        }


        public Line(float rho, float theta)
        {
            var a = Math.Cos(theta);
            var b = Math.Sin(theta);

            var x0 = a * rho;
            var y0 = b * rho;

            StartPoint = new V2d(x0 + 1000 * -b, y0 + 1000 * a);
            EndPoint = new V2d(x0 - 1000 * -b, y0 - 1000 * a);

            InternalLine = new Line2d(StartPoint, EndPoint);
        }

        protected double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        public bool IsValid()
        {
            var angle = Math.Round(
                Math.Abs(RadianToDegree(Math.Atan2(EndPoint.Y - StartPoint.Y, EndPoint.X - StartPoint.X))),
                0
            );

            if (180 - AngleThreshold <= angle || angle <= 0 + AngleThreshold)
                return false;
            if (90 - AngleThreshold <= angle && angle <= 90 + AngleThreshold)
                return false;
            return true;
        }

        public Point Intersect(Line line)
        {
            InternalLine.Intersects(line.InternalLine, out var intersection);

            if (intersection.X.IsNaN() || intersection.Y.IsNaN())
                return null;

            return new Point(intersection);
        }
    }
}