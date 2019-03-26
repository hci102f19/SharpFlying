using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aardvark.Base;
using Geometry.Exceptions;

namespace Geometry.Base
{
    public class Line
    {
        protected Line2d InternalLine;
        public V2d StartPoint;
        public V2d EndPoint;

        protected int AngleThreshold = 20;

        public Line(PointF point) : this(point.X, point.Y)
        {
        }


        public Line(float rho, float theta)
        {
            //TODO: Check for rho is Nan

            double a = Math.Cos(theta);
            double b = Math.Sin(theta);

            double x0 = a * rho;
            double y0 = b * rho;

            StartPoint = new V2d(x0 + 1000 * (-b), y0 + 1000 * a);
            EndPoint = new V2d(x0 - 1000 * (-b), y0 - 1000 * a);

            InternalLine = new Line2d(StartPoint, EndPoint);

            Validate();
        }

        protected double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        protected void Validate()
        {
            double angle = Math.Round(
                Math.Abs(RadianToDegree(Math.Atan2(EndPoint.Y - StartPoint.Y, EndPoint.X - StartPoint.X))),
                0
            );

            if (180 - AngleThreshold <= angle || angle <= 0 + AngleThreshold)
                throw new InvalidLineException("Line not within angle scope");
            else if (90 - AngleThreshold <= angle && angle <= 90 + AngleThreshold)
                throw new InvalidLineException("Line not within angle scope");
        }

        public Point Intersect(Line line)
        {
            V2d intersection = new V2d();

            InternalLine.Intersects(line.InternalLine, out intersection);

            if (intersection.X.IsNaN() || intersection.Y.IsNaN())
                return null;

            return new Point(intersection);
        }
    }
}