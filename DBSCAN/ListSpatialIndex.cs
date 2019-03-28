﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geometry.Base;
using Geometry.Interfaces;

namespace DBSCAN
{
    public class ListSpatialIndex<T> : ISpatialIndex<T> where T : IPointData
    {
        public delegate double DistanceFunction(in Point a, in Point b);

        private IReadOnlyList<T> list;
        private DistanceFunction distanceFunction;

        public ListSpatialIndex(IEnumerable<T> data)
            : this(data, EuclideanDistance)
        {
        }

        public ListSpatialIndex(IEnumerable<T> data, DistanceFunction distanceFunction)
        {
            this.list = data.ToList();
            this.distanceFunction = distanceFunction;
        }

        public static double EuclideanDistance(in Point a, in Point b)
        {
            var xDist = b.X - a.X;
            var yDist = b.Y - a.Y;
            return Math.Sqrt(xDist * xDist + yDist * yDist);
        }

        public IReadOnlyList<T> Search() => list;

        public IReadOnlyList<T> Search(in Point p, double epsilon)
        {
            var l = new List<T>();
            foreach (var q in list)
                if (distanceFunction(p, q.Point) < epsilon)
                    l.Add(q);
            return l;
        }
    }
}