using System;
using System.Collections.Generic;
using System.Linq;
using Geometry.Base;

namespace DBSCANLib
{
    public class ListSpatialIndex
    {
        public delegate bool DistanceFunction(in Point a, in Point b, in double epsilon);

        private readonly DistanceFunction _distanceFunction;

        private readonly IReadOnlyList<PointInfo> _points;

        public ListSpatialIndex(IEnumerable<PointInfo> data)
            : this(data, EuclideanDistance)
        {
        }

        public ListSpatialIndex(IEnumerable<PointInfo> data, DistanceFunction distanceFunction)
        {
            _points = data.ToList();
            _distanceFunction = distanceFunction;
        }

        public static bool EuclideanDistance(in Point a, in Point b, in double epsilon)
        {
            double xDist = b.X - a.X;
            double yDist = b.Y - a.Y;

            if (Math.Abs(xDist) > epsilon || Math.Abs(yDist) > epsilon)
            {
                return false;
            }

            return a.Distance(b) < epsilon;
        }

        public IReadOnlyList<PointInfo> Search()
        {
            return _points;
        }

        public IReadOnlyList<PointInfo> Search(in Point p, double epsilon)
        {
            List<PointInfo> neighbours = new List<PointInfo>();
            foreach (PointInfo point in _points)
            {
                if (_distanceFunction(p, point.Point, epsilon))
                {
                    neighbours.Add(point);
                }
            }

            return neighbours;
        }
    }
}