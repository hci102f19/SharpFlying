﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geometry.Base;
using Geometry.Interfaces;

namespace Geometry.Extended
{
    public class Cluster<T> where T : IPointData
    {
        public List<T> Points = new List<T>();

        public int ClusterSize { get; protected set; } = 0;
        protected double? ClusterDensity = null;

        protected int Modifier = 3;

        public Box GetBoundingBox()
        {
            return new Box(
                Points.Min(point => point.X),
                Points.Min(point => point.Y),
                Points.Max(point => point.X),
                Points.Max(point => point.Y)
            );
        }

        public double Density()
        {
            if (ClusterDensity == null)
            {
                double area = GetBoundingBox().Area;

                if (area > 0 && Math.Log(area) > 0)
                    ClusterDensity = Math.Log((int)ClusterSize) * Modifier / Math.Log(area);
                else
                    ClusterDensity = 0;
            }

            return (double)ClusterDensity;
        }

        public Point GetMean()
        {
            return new Point(
                Points.Average(point => point.X),
                Points.Average(point => point.Y)
            );
        }
    }
}