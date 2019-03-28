﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aardvark.Base;
using Geometry.Extended;
using SDPoint = System.Drawing.Point;

namespace Geometry.Base
{
    public class Point
    {
        protected V2d InternalPoint;

        public double X => InternalPoint.X;
        public double Y => InternalPoint.Y;

        public Point(V2d point) : this(point.X, point.Y)
        {
        }

        public Point(double x, double y)
        {
            InternalPoint = new V2d(x, y);
        }

        public double Distance(Point point)
        {
            return Math.Sqrt(point.InternalPoint.X * InternalPoint.X + InternalPoint.Y * point.InternalPoint.Y);
        }

        public SDPoint AsPoint()
        {
            return new SDPoint((int)X, (int)Y);
        }
    }
}