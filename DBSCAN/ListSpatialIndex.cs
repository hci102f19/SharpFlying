using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geometry.Base;
using Geometry.Interfaces;

namespace DBSCAN
{
    public class ListSpatialIndex<T> : ISpatialIndex<T> where T : IPointData
    {
        public delegate bool DistanceFunction(in Point a, in Point b, in double epsilon);

        private readonly IReadOnlyList<T> _points;
        private readonly DistanceFunction distanceFunction;

        public ListSpatialIndex(IEnumerable<T> data)
            : this(data, EuclideanDistance)
        {
        }

        public ListSpatialIndex(IEnumerable<T> data, DistanceFunction distanceFunction)
        {
            this._points = data.ToList();
            this.distanceFunction = distanceFunction;
        }

        public static bool EuclideanDistance(in Point a, in Point b, in double epsilon)
        {
            var xDist = b.X - a.X;
            var yDist = b.Y - a.Y;
            if (Math.Abs(xDist) > epsilon || Math.Abs(yDist) > epsilon)
                return false;
            return Math.Sqrt(xDist * xDist + yDist * yDist) < epsilon;
        }

        public IReadOnlyList<T> Search() => _points;

        public IReadOnlyList<T> Search(in Point p, double epsilon)
        {
            var neighbours = new List<T>();
            foreach (var point in _points)
                if (distanceFunction(p, point.Point, epsilon))
                    neighbours.Add(point);
            return neighbours;
        }
    }
}