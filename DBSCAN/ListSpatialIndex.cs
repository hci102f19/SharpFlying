using System;
using System.Collections.Generic;
using System.Linq;
using Geometry.Base;

namespace DBSCAN
{
    public class ListSpatialIndex
    {
        public delegate bool DistanceFunction(in Point a, in Point b, in double epsilon);

        private readonly IReadOnlyList<PointInfo> _points;
        private readonly DistanceFunction distanceFunction;

        public ListSpatialIndex(IEnumerable<PointInfo> data)
            : this(data, EuclideanDistance)
        {
        }

        public ListSpatialIndex(IEnumerable<PointInfo> data, DistanceFunction distanceFunction)
        {
            _points = data.ToList();
            this.distanceFunction = distanceFunction;
        }

        public static bool EuclideanDistance(in Point a, in Point b, in double epsilon)
        {
            var xDist = b.X - a.X;
            var yDist = b.Y - a.Y;

            if (Math.Abs(xDist) > epsilon || Math.Abs(yDist) > epsilon)
                return false;
            return a.Distance(b) < epsilon;
        }

        public IReadOnlyList<PointInfo> Search()
        {
            return _points;
        }

        public IReadOnlyList<PointInfo> Search(in Point p, double epsilon)
        {
            var neighbours = new List<PointInfo>();
            foreach (var point in _points)
                if (distanceFunction(p, point.Point, epsilon))
                    neighbours.Add(point);
            return neighbours;
        }
    }
}