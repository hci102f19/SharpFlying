using System;
using System.Collections.Generic;
using System.Linq;
using Geometry.Base;

namespace Geometry.Extended
{
    public class Cluster
    {
        protected double? ClusterDensity;

        protected int Modifier = 3;
        public List<PointContainer> Points = new List<PointContainer>();

        public int ClusterSize { get; protected set; } = 0;

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
                var area = GetBoundingBox().Area;

                if (area > 0 && Math.Log(area) > 0)
                    ClusterDensity = Math.Log(ClusterSize) * Modifier / Math.Log(area);
                else
                    ClusterDensity = 0;
            }

            return (double) ClusterDensity;
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