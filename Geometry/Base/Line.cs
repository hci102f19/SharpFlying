using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aardvark.Base;

namespace Geometry.Base
{
    public class Line
    {
        protected Polygon2d InternalLine;
        protected V2d StartPoint;
        protected V2d EndPoint;

        protected int AngleThreshold = 20;

        public Line(float rho, float theta)
        {
            //TODO: Check for rho is Nan

            var a = Math.Cos(theta);
            var b = Math.Sin(theta);

            var x0 = a * rho;
            var y0 = b * rho;

            StartPoint = new V2d(x0 + 1000 * (-b), y0 + 1000 * a);
            EndPoint = new V2d(x0 - 1000 * (-b), y0 - 1000 * a);

            InternalLine = new Polygon2d(StartPoint, EndPoint);

            Validate();
        }

        protected double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        protected void Validate()
        {
            var angle = Math.Round(Math.Abs(RadianToDegree(Math.Atan2(EndPoint.Y - StartPoint.Y, EndPoint.X - StartPoint.X))), 0);

            if (180 - AngleThreshold <= angle || angle <= 0 + AngleThreshold)
                throw new Exception("Angle Error");
            else if (90 - AngleThreshold <= angle || angle <= 90 + AngleThreshold)
                throw new Exception("Angle Error");
        }
    }
}